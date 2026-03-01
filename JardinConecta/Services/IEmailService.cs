namespace JardinConecta.Services
{
    public interface IEmailService
    {
        Task<Result> SendAsync(string to, string subject, string body, bool isHtml = false);
    }
}
