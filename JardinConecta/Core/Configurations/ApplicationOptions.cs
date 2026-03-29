namespace JardinConecta.Core.Configurations
{
    public class ApplicationOptions
    {
        public string Name { get; set; }
        public string Version { get; set; }
        public string BaseUrl { get; set; }
        public static string Section => "Application";
    }
}
