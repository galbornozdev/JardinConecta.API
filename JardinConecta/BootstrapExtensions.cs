using JardinConecta.Configurations;

namespace JardinConecta
{
    public static class BootstrapExtensions
    {
        public static void ConfigureAppOptions(this IServiceCollection services, IConfiguration config)
        {
            services.Configure<JwtOptions>(config.GetSection(JwtOptions.Section));
            services.Configure<EmailOptions>(config.GetSection(EmailOptions.Section));
        }
    }
}
