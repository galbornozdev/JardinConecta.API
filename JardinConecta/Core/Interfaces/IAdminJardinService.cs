namespace JardinConecta.Core.Interfaces
{
    public interface IAdminJardinService
    {
        Task<Guid> SelectIdJardin(HttpContext httpContext, Guid? idJardin);
    }
}