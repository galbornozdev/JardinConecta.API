using JardinConecta.Models.Email;

namespace JardinConecta.Services
{
    public interface IEmailService
    {
        Task<Result> SendAsync(string to, string subject, string body, bool isHtml = false);
        Task<Result> SendTemplateAsync(string to, BaseEmailModel emailModel);
    }
}
