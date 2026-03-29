namespace JardinConecta.Core.Configurations
{
    public class TwilioOptions
    {
        public string AccountSid { get; set; } = default!;
        public string AuthToken { get; set; } = default!;
        public string FromPhoneNumber { get; set; } = default!;
        public static string Section => "Twilio";
    }
}