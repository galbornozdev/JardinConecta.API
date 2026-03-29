using JardinConecta.Services.Application.Dtos;

namespace JardinConecta.Services.Application.Interfaces
{
    public interface IAuthService
    {
        Task<LoginResult> Login(string email, string password);
    }
}
