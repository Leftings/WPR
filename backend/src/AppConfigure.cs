using System.Security.Policy;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Options;
using WPR.Controllers.Cookie;
using WPR.Cryption;
using WPR.Data;
using WPR.Database;
using WPR.Repository;
using WPR.Hashing;
using System.Net;

namespace WPR;

/// <summary>
/// Deze class is responsible voor het configureren van de application services en database connectie
/// Het heeft methodes om de database te initializeren en het configureren van de web applicatie.
/// </summary>
public class AppConfigure
{
    public static void InitDatabase(IServiceProvider services)
    {
        var dbConnector = services.GetRequiredService<Connector>();
        try
        {
            using (var connection = dbConnector.CreateDbConnection())
            {
                Console.WriteLine("Database connection established");
            }
        }
        catch (Exception e)
        {
            Console.WriteLine("Failed to establish database connection");
            Console.WriteLine(e.StackTrace);
        }
    }

    public static WebApplication ConfigureApplication(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "development";
        builder.Configuration.AddEnvironmentVariables();

        Console.WriteLine(Environment.GetEnvironmentVariable("ASPNETCORE_URLS"));
        if (environment == "development")
        {
            builder.Configuration.AddJsonFile(".env", optional: true, reloadOnChange: true);
        }
        else if (environment == "Production")
        {
            builder.Configuration.AddJsonFile(".env.deployment", optional: true, reloadOnChange: true);
        }

        var cookiePolicyOptions = new CookiePolicyOptions
        {
            MinimumSameSitePolicy = SameSiteMode.Strict
        };

        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowSpecificOrigins", policy =>
            {
                policy.WithOrigins(
                    "https://carandall.nl",
                    "https://95.99.30.110:8443",
                    "http://localhost:5173",
                    "http://95.99.30.110:8080",
                    "http://wpr_backend_1:5000",  // Include backend address in Docker network
                    "http://wpr_frontend_1:3000",
                    "http://wpr_nginx_1:8080")
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials();
            });
        });

        // Remove Kestrel configuration: Default to use the Docker internal network and port 5000
        builder.WebHost.UseUrls("http://0.0.0.0:5000"); // Listen on all interfaces and port 5000

        builder.Services.AddSingleton<EnvConfig>(); // Singleton for environment configuration
        builder.Services.AddTransient<Connector>(); // Transient for database connection
        builder.Services.AddScoped<IUserRepository, UserRepository>(); // Scoped for user repository
        builder.Services.AddScoped<SessionHandler>(); // Scoped session handler
        builder.Services.AddScoped<Crypt>();
        builder.Services.AddScoped<Hashing.Hash>();

        builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(options =>
            {
                options.ExpireTimeSpan = TimeSpan.FromMinutes(5);
                options.SlidingExpiration = true;
                options.AccessDeniedPath = "/Forbidden/";
            });

        builder.Services.AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
            });

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        var app = builder.Build();

        if (app.Environment.IsProduction())
        {
            app.UseCors("AllowSpecificOrigins");
        }

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
                c.RoutePrefix = string.Empty;
            });
        }

        app.UseCors("AllowSpecificOrigins");

        // Don't use HTTPS redirection
        // app.UseHttpsRedirection(); // Comment out or remove this line to disable HTTPS redirection

        app.MapControllers();
        app.UseAuthorization();
        app.UseAuthentication();
        app.UseCookiePolicy(cookiePolicyOptions);

        app.MapDefaultControllerRoute();

        return app;
    }
}
