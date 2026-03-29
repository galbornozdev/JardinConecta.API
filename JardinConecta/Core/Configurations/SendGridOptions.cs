namespace JardinConecta.Core.Configurations
{
    public class SendGridOptions
    {
        public string ApiKey { get; set; } = default!;
        public string FromEmail { get; set; } = default!;
        public string FromName { get; set; } = default!;
        public static string Section => "SendGrid";
    }
}