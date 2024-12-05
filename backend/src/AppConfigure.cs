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
    /// <summary>
    /// Configureer de web applicatie, zoals middleware, services, authentication en CORS settings.
    /// </summary>
    /// <param name="args">De command line arguments passed to the application at startup.</param>
    /// <returns>De geconfigureerde WebApplication instantie.</returns>
    public static WebApplication ConfigureApplication(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "development";
        builder.Configuration.AddEnvironmentVariables();

        if (environment == "development")
        {
            builder.Configuration.AddJsonFile(".env", optional: true, reloadOnChange: true);
        }
        else if (environment == "Production")
        {
            builder.Configuration.AddJsonFile(".env.deployment", optional: true, reloadOnChange: true);
        }
        
        // Configure cookie policy options
        var cookiePolicyOptions = new CookiePolicyOptions
        {
            MinimumSameSitePolicy = SameSiteMode.Strict //  Set the strict sametime policy voor de cookies
        };

        // Configureer CORS om requests van de specifieke port: "https://localhost:5173" toe te staan met Any method en credentials.
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowLocalhost", policy =>
            {
                policy.WithOrigins("http://localhost:5173")  // Development URL
                    .AllowAnyHeader()
                    .AllowCredentials()
                    .AllowAnyMethod();
            });

            // For production
            options.AddPolicy("AllowProduction", policy =>
            {
                policy.WithOrigins("http://carandall.nl", "https://carandall.nl") // Production URL
                    .AllowAnyHeader()
                    .AllowCredentials()
                    .AllowAnyMethod();
            });
        });

        
        // Registreer services voor Dependency Injection.
        builder.Services.AddSingleton<EnvConfig>(); // Singleton voor environment configuration
        builder.Services.AddTransient<Connector>(); // Transient voor database connection.
        builder.Services.AddScoped<IUserRepository, UserRepository>(); // Scoped voor user repository
        //builder.Services.AddScoped<IResponseCookies>();
        builder.Services.AddScoped<SessionHandler>(); // Scoped session handler
        builder.Services.AddScoped<Crypt>();
        builder.Services.AddScoped<Hashing.Hash>();
        
        // Configureer authenticatie met cookie-based authenticatie schema.
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
                options.JsonSerializerOptions.ReferenceHandler =
                    ReferenceHandler.IgnoreCycles;
            });
        
        // Services voor Swagger API
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        
        var app = builder.Build();

        if (app.Environment.IsProduction())
        {
            app.UseCors("AllowProduction");
        }
        else
        {
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
                    c.RoutePrefix = string.Empty;
                });
            }
            app.UseCors("AllowLocalhost");
        }
        
        app.UseHttpsRedirection();
        app.MapControllers();
        app.UseAuthorization();
        app.UseAuthentication();
        app.UseCookiePolicy(cookiePolicyOptions);

        //app.MapRazorPages();
        app.MapDefaultControllerRoute();

        return app;
    }
}
