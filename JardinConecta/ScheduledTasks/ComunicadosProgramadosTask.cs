using JardinConecta.Core.Entities;
using JardinConecta.Core.Interfaces;

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
            var comunicadosProgramadosService = scope.ServiceProvider.GetRequiredService<IComunicadosProgramadosService>();

            await comunicadosProgramadosService.PublicarComunicadosProgramados(stoppingToken);
        }
    }
}
