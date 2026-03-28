
namespace JardinConecta.Services.Application
{
    public interface IOnboardingService
    {
        Task AltaDeUsuario(string email, string password);
    }
}