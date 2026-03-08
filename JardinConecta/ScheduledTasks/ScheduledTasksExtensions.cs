using JardinConecta.Infrastructure.Repository;
using JardinConecta.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace JardinConecta.ScheduledTasks
{
    public static class ScheduledTasksExtensions
    {
        public static async Task RegisterScheduledTasksAsync(this WebApplication app)
        {
            using var scope = app.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ServiceContext>();

            var tasks = await context.Set<ScheduledTask>()
                .Where(t => t.Habilitado)
                .ToListAsync();

            var workers = app.Services.GetServices<IHostedService>()
                .OfType<ScheduledWorker>()
                .ToList();

            foreach (var task in tasks)
            {
                var worker = workers.FirstOrDefault(w => (int)w.TaskId == task.Id);
                if (worker != null)
                {
                    worker.SetCron(task.Cron);
                    app.Logger.LogInformation("Tarea {TaskName} registrada con cron: {Cron}", task.Nombre, task.Cron);
                }
                else
                {
                    app.Logger.LogWarning("No se encontró worker para la tarea id: {TaskId}", task.Id);
                }
            }
        }
    }
}
