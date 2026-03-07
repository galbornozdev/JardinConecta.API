namespace JardinConecta.Configurations
{
    public class FirebaseOptions
    {
        public const string Section = "Firebase";
        public string Type { get; set; } = string.Empty;
        public string ProjectId { get; set; } = string.Empty;
        public string PrivateKeyId { get; set; } = string.Empty;
        public string PrivateKey { get; set; } = string.Empty;
        public string ClientEmail { get; set; } = string.Empty;
        public string ClientId { get; set; } = string.Empty;
        public string AuthUri { get; set; } = string.Empty;
        public string TokenUri { get; set; } = string.Empty;
    }
}
