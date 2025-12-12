namespace JardinConecta.Services
{
    public interface IJwtService
    {
        (string, DateTime) GenerateToken(Guid userId, string email, string role, Guid? IdJardin = null);
    }
}