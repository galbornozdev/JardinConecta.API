using JardinConecta.Infrastructure.Repository;
using JardinConecta.Models.Entities;
using JardinConecta.Services.Application;
using Microsoft.EntityFrameworkCore;

namespace JardinConecta.ScheduledTasks
{
    public class ComunicadosProgramadosTask : ScheduledWorker
    {
        public override ScheduledTaskId TaskId => ScheduledTaskId.ComunicadosProgramados;
        public override string TaskName => "ComunicadosProgramados";

        public ComunicadosProgramadosTask(
            IServiceScopeFactory scopeFactory,
            ILogger<ComunicadosProgramadosTask> logger)
            : base(scopeFactory, logger)
        {
        }

        protected override async Task RunAsync(IServiceScope scope, CancellationToken stoppingToken)
        {
            var context = scope.ServiceProvider.GetRequiredService<ServiceContext>();
            var salaNotificationService = scope.ServiceProvider.GetRequiredService<ISalaNotificationService>();

            var now = DateTime.UtcNow;

            var pendientes = await context.Set<Comunicado>()
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

            await context.SaveChangesAsync(stoppingToken);

            foreach (var comunicado in pendientes)
                await salaNotificationService.NotificarAsync(
                    comunicado.IdSala,
                    "Nuevo comunicado",
                    comunicado.Titulo,
                    data: new Dictionary<string, string>
                    {
                        { "type", "comunicado" },
                        { "comunicadoId", comunicado.Id.ToString() }
                    });

            Logger.LogInformation("Scheduler: {Count} comunicado(s) publicados", pendientes.Count);
        }
    }
}
