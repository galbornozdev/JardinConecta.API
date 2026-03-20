using JardinConecta.Services;

namespace JardinConecta.Services.Infrastructure
{
    public interface INotificationService
    {
        Task<Result> SendPushAsync(string deviceToken, string title, string body, Dictionary<string, string>? data = null);
    }
}
