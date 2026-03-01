using JardinConecta.Configurations;
using JardinConecta.Services;
using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace JardinConecta.Infrastructure
{
    public class SendGridEmailService : IEmailService
    {
        private EmailOptions _emailOptions;
        private SendGridClient _client;

        public SendGridEmailService(IOptions<EmailOptions> emailOptions)
        {
            _emailOptions = emailOptions.Value;
            _client = new SendGridClient(_emailOptions.ApiKey);
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
    }
}
