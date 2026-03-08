using JardinConecta.Models.ViewModels.EmailTemplates;
using JardinConecta.Services;

namespace JardinConecta.Services.Infrastructure
{
    public interface IEmailService
    {
        Task<Result> SendAsync(string to, string subject, string body, bool isHtml = false);
        Task<Result> SendTemplateAsync(string to, BaseEmailModel emailModel);
    }
}
