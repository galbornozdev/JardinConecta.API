using JardinConecta.Core.Configurations;
using JardinConecta.Core.Interfaces.Infrastructure;
using JardinConecta.Core.Services.Dtos;
using JardinConecta.Models.ViewModels.EmailTemplates;
using Microsoft.Extensions.Options;
using RazorLight;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace JardinConecta.Infrastructure
{
    public class SendGridEmailService : IEmailService
    {
        private SendGridOptions _emailOptions;
        private SendGridClient _client;
        private RazorLightEngine _engine;

        public SendGridEmailService(IOptions<SendGridOptions> emailOptions)
        {
            _emailOptions = emailOptions.Value;
            _client = new SendGridClient(_emailOptions.ApiKey);
            _engine = new RazorLightEngineBuilder()
                .UseFileSystemProject(Path.Combine(Directory.GetCurrentDirectory(), "EmailTemplates"))
                .UseMemoryCachingProvider()
                .Build();
        }
        public async Task<Result> SendAsync(string to, string subject, string body, bool isHtml = false)
        {
            var from = new EmailAddress(_emailOptions.FromEmail, _emailOptions.FromName);
            var toEmail = new EmailAddress(to);

            var msg = new SendGridMessage()
            {
                From = from,
                Subject = subject,
                HtmlContent = isHtml ? body : null,
                PlainTextContent = isHtml ? null : body
            };

            msg.AddTo(toEmail);

            var response = await _client.SendEmailAsync(msg);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Body.ReadAsStringAsync();
                return Result.Failure($"Failed to send email: {response.StatusCode} - {error}");
            }

            return Result.Success();
        }

        public async Task<Result> SendTemplateAsync(string to, BaseEmailModel emailModel)
        {
            var htmlBody = await RenderTemplateAsync(emailModel);

            return await SendAsync(to, emailModel.Subject, htmlBody, true);
        }

        private async Task<string> RenderTemplateAsync<T>(string templatePath, T model)
        {
            return await _engine.CompileRenderAsync(templatePath, model);
        }

        private async Task<string> RenderTemplateAsync<T>(T model)
        {
            var filename = GetTemplateHtmlFileName(model);
            return await RenderTemplateAsync(filename, model);
        }

        private string GetTemplateHtmlFileName<T>(T model)
        {
            var modelType = model!.GetType();
            var templateName = modelType.Name.EndsWith("ViewModel")
                ? modelType.Name.Replace("ViewModel", "")
                : modelType.Name;

            return $"{templateName}.cshtml";
        }
    }
}
