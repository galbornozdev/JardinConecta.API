namespace JardinConecta.Services
{
    public interface ISmsService
    {
        Task<Result> SendAsync(string to, string message);
    }
}
