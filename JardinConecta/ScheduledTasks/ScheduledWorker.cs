using Cronos;
using JardinConecta.Infrastructure.Repository;
using JardinConecta.Models.Entities;

namespace JardinConecta.ScheduledTasks
{
    public abstract class ScheduledWorker : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        protected readonly ILogger Logger;
        private string? _cron;

        public abstract ScheduledTaskId TaskId { get; }
        public abstract string TaskName { get; }
        protected abstract Task RunAsync(IServiceScope scope, CancellationToken stoppingToken);

        protected ScheduledWorker(IServiceScopeFactory scopeFactory, ILogger logger)
        {
            _scopeFactory = scopeFactory;
            Logger = logger;
        }

        public void SetCron(string cron) => _cron = cron;

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (_cron == null)
            {
                Logger.LogInformation("Tarea {TaskName} deshabilitada, no se ejecutará", TaskName);
                return;
            }

            var cronExpression = CronExpression.Parse(_cron);

            while (!stoppingToken.IsCancellationRequested)
            {
                var next = cronExpression.GetNextOccurrence(DateTimeOffset.UtcNow, TimeZoneInfo.Utc);
                if (!next.HasValue) break;

                var delay = next.Value - DateTimeOffset.UtcNow;
                if (delay > TimeSpan.Zero)
                    await Task.Delay(delay, stoppingToken);

                if (stoppingToken.IsCancellationRequested) break;

                try
                {
                    Logger.LogInformation("Ejecutando tarea {TaskName}", TaskName);
                    using var scope = _scopeFactory.CreateScope();
                    await RunAsync(scope, stoppingToken);
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, "Error en tarea {TaskName}", TaskName);
                }
            }
        }
    }
}
