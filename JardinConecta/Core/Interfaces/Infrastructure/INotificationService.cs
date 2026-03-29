using JardinConecta.Core.Services.Dtos;

namespace JardinConecta.Core.Interfaces.Infrastructure
{
    public interface INotificationService
    {
        Task<Result> SendPushAsync(string deviceToken, string title, string body, Dictionary<string, string>? data = null);
    }
}
