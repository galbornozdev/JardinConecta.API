using JardinConecta.Infrastructure.Repository;
using JardinConecta.Models.Entities;
using JardinConecta.Services.Application;
using Microsoft.EntityFrameworkCore;

namespace JardinConecta.ScheduledTasks
{
    public class ComunicadosProgramadosTask : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<ComunicadosProgramadosTask> _logger;

        public ComunicadosProgramadosTask(
            IServiceScopeFactory scopeFactory,
            ILogger<ComunicadosProgramadosTask> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await PublicarProgramadosAsync();
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }

        private async Task PublicarProgramadosAsync()
        {
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ServiceContext>();
            var salaNotificationService = scope.ServiceProvider.GetRequiredService<ISalaNotificationService>();

            var now = DateTime.UtcNow;

            var pendientes = await context.Set<Comunicado>()
                .Where(x => x.Estado == (int)EstadoComunicado.Programado && x.FechaPrograma <= now)
                .ToListAsync();

            if (pendientes.Count == 0) return;

            foreach (var comunicado in pendientes)
            {
                comunicado.Estado = (int)EstadoComunicado.Publicado;
                comunicado.FechaPublicacion = now;
                comunicado.FechaPrograma = null;
                comunicado.UpdatedAt = now;
            }

            await context.SaveChangesAsync();

            foreach (var comunicado in pendientes)
                await salaNotificationService.NotificarAsync(comunicado.IdSala, "Nuevo comunicado", comunicado.Titulo);

            _logger.LogInformation("Scheduler: {Count} comunicado(s) publicados", pendientes.Count);
        }
    }
}
