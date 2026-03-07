using JardinConecta.Services;

namespace JardinConecta.Services
{
    public interface INotificationService
    {
        Task<Result> SendPushAsync(string deviceToken, string title, string body);
    }
}
