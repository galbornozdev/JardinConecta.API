namespace JardinConecta.Services
{
    public interface ITokenService
    {
        (string, DateTime) GenerateToken(Guid userId, string email, string role, Guid? IdJardin = null);
    }
}