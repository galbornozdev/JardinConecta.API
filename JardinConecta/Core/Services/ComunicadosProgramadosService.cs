using JardinConecta.Core.Entities;
using JardinConecta.Core.Interfaces;
using JardinConecta.Infrastructure.Repository;
using Microsoft.EntityFrameworkCore;

namespace JardinConecta.Core.Services
{
    public class ComunicadosProgramadosService : IComunicadosProgramadosService
    {
        private readonly ServiceContext _context;
        private ISalaNotificationService _salaNotificationService;
        private readonly ILogger<ComunicadosProgramadosService> _logger;

        public ComunicadosProgramadosService(
            ServiceContext context,
            ISalaNotificationService salaNotificationService,
            ILogger<ComunicadosProgramadosService> logger
        )
        {
            _context = context;
            _salaNotificationService = salaNotificationService;
            _logger = logger;
        }

        public async Task PublicarComunicadosProgramados(CancellationToken stoppingToken)
        {
            var now = DateTime.UtcNow;

            var pendientes = await _context.Set<Comunicado>()
                .Where(x => x.Estado == (int)EstadoComunicado.Programado && x.FechaPrograma <= now)
                .ToListAsync(stoppingToken);

            if (pendientes.Count == 0) return;

            foreach (var comunicado in pendientes)
            {
                comunicado.Estado = (int)EstadoComunicado.Publicado;
                comunicado.FechaPublicacion = now;
                comunicado.FechaPrograma = null;
                comunicado.UpdatedAt = now;
            }

            await _context.SaveChangesAsync(stoppingToken);

            foreach (var comunicado in pendientes)
                await _salaNotificationService.NotificarAsync(
                    comunicado.IdSala,
                    "Nuevo comunicado",
                    comunicado.Titulo,
                    data: new Dictionary<string, string>
                    {
                        { "type", "comunicado" },
                        { "comunicadoId", comunicado.Id.ToString() }
                    });

            _logger.LogInformation("Scheduler: {Count} comunicado(s) publicados", pendientes.Count);
        }
    }
}
