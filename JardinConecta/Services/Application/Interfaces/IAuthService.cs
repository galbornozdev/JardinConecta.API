using JardinConecta.Models.Http.Responses;

namespace JardinConecta.Services.Application.Interfaces
{
    public interface IAuthService
    {
        Task<LoginResponse> Login(string email, string password);
    }
}