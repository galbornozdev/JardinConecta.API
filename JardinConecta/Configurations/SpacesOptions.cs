namespace JardinConecta.Configurations
{
    public class SpacesOptions
    {
        public string AccessKey { get; set; } = default!;
        public string SecretKey { get; set; } = default!;
        public string Endpoint { get; set; } = default!;
        public string Region { get; set; } = default!;
        public string BucketName { get; set; } = default!;
        public static string Section => "Spaces";
    }
}
