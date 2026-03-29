using JardinConecta.Core.Configurations;
using JardinConecta.Core.Interfaces;
using JardinConecta.Core.Interfaces.Infrastructure;
using JardinConecta.Core.Services;
using JardinConecta.Infrastructure;
using JardinConecta.ScheduledTasks;

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

        public static void AddAppServices(this IServiceCollection services)
        {
            //infraestructura
            services.AddSingleton<IFileStorageService, SpacesFileStorageService>();
            //services.AddTransient<IFileStorageService, FileLocalStorageService>();
            services.AddScoped<IEmailService, SendGridEmailService>();
            services.AddScoped<ISmsService, TwilioSmsService>();
            services.AddSingleton<INotificationService, FirebaseNotificationService>();
            services.AddSingleton<ITokenService, JwtService>();

            //application
            services.AddScoped<IAdminJardinService, AdminJardinService>();
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IChatService, ChatService>();
            services.AddScoped<ICodigosDeInvitacionService, CodigosDeInvitacionService>();
            services.AddScoped<IComunicadosService, ComunicadosService>();
            services.AddScoped<IInfantesService, InfantesService>();
            services.AddScoped<ISalaNotificationService, SalaNotificationService>();
            services.AddScoped<ISalasService, SalasService>();
            services.AddScoped<IUsuariosService, UsuariosService>();
            services.AddScoped<IComunicadosProgramadosService, ComunicadosProgramadosService>();

            //tareas programadas
            services.AddHostedService<ComunicadosProgramadosTask>();

        }
    }
}
