using JardinConecta.Services;

namespace JardinConecta.Services.Infrastructure
{
    public interface ISmsService
    {
        Task<Result> SendAsync(string to, string message);
    }
}
