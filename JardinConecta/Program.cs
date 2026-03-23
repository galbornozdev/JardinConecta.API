using JardinConecta;
using JardinConecta.Configurations;
using JardinConecta.Middleware;
using Microsoft.Extensions.FileProviders;
using JardinConecta.ScheduledTasks;
using JardinConecta.Infrastructure;
using JardinConecta.Infrastructure.Repository;
using JardinConecta.Services;
using JardinConecta.Services.Application;
using JardinConecta.Services.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using Serilog.Formatting.Json;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((ctx, lc) =>
    lc.ReadFrom.Configuration(ctx.Configuration)
      .Enrich.FromLogContext()
      .WriteTo.Console(new JsonFormatter()));

builder.Services.AddHttpContextAccessor();

// Add configurations
builder.Services.ConfigureAppOptions(builder.Configuration);

// Add services to the container.
builder.Services.AddSingleton<ITokenService, JwtService>();
builder.Services.AddTransient<IFileStorageService, FileLocalStorageService>();
builder.Services.AddScoped<IEmailService, SendGridEmailService>();
builder.Services.AddScoped<ISmsService, TwilioSmsService>();
builder.Services.AddSingleton<INotificationService, FirebaseNotificationService>();
builder.Services.AddScoped<ISalaNotificationService, SalaNotificationService>();
builder.Services.AddHostedService<ComunicadosProgramadosTask>();

// Health checks
builder.Services.AddHealthChecks()
    .AddNpgSql(builder.Configuration.GetConnectionString("Default")!, name: "database");

// Use Postgress database
builder.Services.AddDbContext<ServiceContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Default")));

var jwtKey = builder.Configuration["Jwt:Key"];
var jwtIssuer = builder.Configuration["Jwt:Issuer"];

builder.Services
    .AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ClockSkew = TimeSpan.Zero // no extra time
        };
    });

builder.Services.AddAuthorization();

var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod());
});

//builder.Services.AddControllers();
builder.Services.AddControllersWithViews();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options => {
    options.SwaggerDoc("v1", new() { Title = "JardinConecta.API", Version = "v1" });

    // 🔐 Add Bearer auth to Swagger
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter your JWT token here. Example: Bearer {token}"
    });

    // 🔐 Require Bearer token globally (optional but recommended)
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

app.UseStaticFiles();

var mediaPath = Path.Combine(app.Environment.ContentRootPath, "media");
Directory.CreateDirectory(mediaPath);
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(mediaPath),
    RequestPath = "/media"
});

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();

app.UseMiddleware<ErrorHandlingMiddleware>();

var httpLoggingOptions = app.Configuration
    .GetSection("HttpLogging")
    .Get<HttpLoggingOptions>() ?? new HttpLoggingOptions();

if (httpLoggingOptions.Enabled)
{
    app.UseMiddleware<HttpLoggingMiddleware>(httpLoggingOptions);
}

app.UseAuthorization();

app.MapControllers();

app.MapHealthChecks("/Health/Live");
app.MapHealthChecks("/Health/Ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = check => check.Name == "database"
});

await app.RegisterScheduledTasksAsync();

app.Run();