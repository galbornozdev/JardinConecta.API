using JardinConecta.Core.Services.Dtos;

namespace JardinConecta.Core.Interfaces.Infrastructure
{
    public interface ISmsService
    {
        Task<Result> SendAsync(string to, string message);
    }
}
