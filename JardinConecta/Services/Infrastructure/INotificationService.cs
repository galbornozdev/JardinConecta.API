using JardinConecta.Services;

namespace JardinConecta.Services.Infrastructure
{
    public interface INotificationService
    {
        Task<Result> SendPushAsync(string deviceToken, string title, string body);
    }
}
