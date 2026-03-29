using JardinConecta.Core.Services.Dtos;

namespace JardinConecta.Core.Interfaces
{
    public interface IAuthService
    {
        Task<LoginResult> Login(string email, string password);
    }
}
