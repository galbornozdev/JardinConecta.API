using JardinConecta.Core.Services.Dtos;
using JardinConecta.Models.ViewModels.EmailTemplates;

namespace JardinConecta.Core.Interfaces.Infrastructure
{
    public interface IEmailService
    {
        Task<Result> SendAsync(string to, string subject, string body, bool isHtml = false);
        Task<Result> SendTemplateAsync(string to, BaseEmailModel emailModel);
    }
}
