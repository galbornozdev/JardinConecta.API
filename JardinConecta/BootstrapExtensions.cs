using JardinConecta.Configurations;

namespace JardinConecta
{
    public static class BootstrapExtensions
    {
        public static void ConfigureAppOptions(this IServiceCollection services, IConfiguration config)
        {
            services.Configure<ApplicationOptions>(config.GetSection(ApplicationOptions.Section));
            services.Configure<JwtOptions>(config.GetSection(JwtOptions.Section));
            services.Configure<SendGridOptions>(config.GetSection(SendGridOptions.Section));
            services.Configure<TwilioOptions>(config.GetSection(TwilioOptions.Section));
            services.Configure<FirebaseOptions>(config.GetSection(FirebaseOptions.Section));
            services.Configure<SpacesOptions>(config.GetSection(SpacesOptions.Section));
        }
    }
}
