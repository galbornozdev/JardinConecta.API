namespace JardinConecta.Services.Application.Interfaces
{
    public interface IAdminJardinService
    {
        Task<Guid> SelectIdJardin(HttpContext httpContext, Guid? idJardin);
    }
}