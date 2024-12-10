using System.Security.Policy;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Options;
using Microsoft.VisualBasic;
using Org.BouncyCastle.Crypto.Prng;
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
    /// <summary>
    /// Initializes the database connection and performs necessary setup.
    /// </summary>
    /// <param name="services">The service provider to retrieve the database connection.</param>
    public static void InitDatabase(IServiceProvider services)
    {
        var dbConnector = services.GetRequiredService<IConnector>();  // Ensure IConnector is used
        
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

    /// <summary>
    /// Configureer de web applicatie, zoals middleware, services, authentication en CORS settings.
    /// </summary>
    /// <param name="args">De command line arguments passed to the application at startup.</param>
    /// <returns>De geconfigureerde WebApplication instantie.</returns>
    public static WebApplication ConfigureApplication(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Set environment and configuration files based on environment variable
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "development";

        // Register services for Dependency Injection
        builder.Services.AddSingleton<EnvConfig>();  // Singleton for environment configuration
        builder.Services.AddTransient<IConnector, Connector>();  // Register IConnector to Connector mapping (transient)
        builder.Services.AddScoped<IUserRepository, UserRepository>();  // Scoped for user repository
        builder.Services.AddScoped<SessionHandler>();  // Scoped session handler
        builder.Services.AddScoped<Crypt>();  // Scoped for Crypt service
        builder.Services.AddScoped<Hashing.Hash>();  // Scoped for Hashing service

        // Configure other services and settings
        builder.Configuration.AddEnvironmentVariables();

        if (environment == "development")
        {
            builder.Configuration.AddJsonFile(".env", optional: true, reloadOnChange: true);
        }
        else if (environment == "Production")
        {
            builder.Configuration.AddJsonFile(".env.deployment", optional: true, reloadOnChange: true);
        }

        // Configure Cookie Policy
        var cookiePolicyOptions = new CookiePolicyOptions
        {
            MinimumSameSitePolicy = SameSiteMode.Strict
        };

        // Register CORS policies
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowSpecificOrigins", policy =>
                policy.WithOrigins("http://95.99.30.110:8080", "http://localhost:5173", "http://www.carandall.nl:8080")
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials()
            );
        });

        // Configure Kestrel server settings based on environment variable for URLs
        var urls = Environment.GetEnvironmentVariable("ASPNETCORE_URLS") ?? "http://0.0.0.0:80"; // Default to port 80 if not set
        Uri uri;

        try
        {
            uri = new Uri(urls);
        }
        catch (UriFormatException)
        {
            Console.WriteLine($"Invalid ASPNETCORE_URLS format: {urls}. Using default URL http://0.0.0.0:80");
            uri = new Uri("http://0.0.0.0:80");
        }

        builder.WebHost.ConfigureKestrel(options =>
        {
            if (uri.Host == "0.0.0.0")
            {
                options.Listen(IPAddress.Any, 5000);  // Default port for "0.0.0.0"
            }
            else
            {
                try
                {
                    options.Listen(IPAddress.Parse(uri.Host), uri.Port);
                }
                catch (FormatException)
                {
                    Console.WriteLine($"Invalid IP address specified: {uri.Host}. Using default binding to all IPs.");
                    options.Listen(IPAddress.Any, uri.Port);
                }
            }
        });

        // Configure authentication with cookie-based authentication schema
        builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(options =>
            {
                options.ExpireTimeSpan = TimeSpan.FromMinutes(5);
                options.SlidingExpiration = true;
                options.AccessDeniedPath = "/Forbidden/";  // Define path for access denied
            });

        // Add controllers and JSON serializer options
        builder.Services.AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
            });

        // Services for Swagger API documentation
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        var app = builder.Build();

        // Enable Swagger and CORS based on environment
        if (app.Environment.IsProduction())
        {
            app.UseCors("AllowSpecificOrigins");  // Use production CORS policy
        }
        else
        {
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
                    c.RoutePrefix = string.Empty;  // Root path for Swagger UI
                });
            }
            app.UseCors("AllowSpecificOrigins");  // Use specific origins CORS policy
        }

        app.UseHttpsRedirection();
        app.MapControllers();  // Map controllers for routing
        app.UseAuthorization();
        app.UseAuthentication();
        app.UseCookiePolicy(cookiePolicyOptions);  // Apply cookie policy

        app.MapDefaultControllerRoute();  // Default route for controllers

        return app;
    }
}
