using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.Cookies;
using WPR.Cryption;
using WPR.Data;
using WPR.Database;
using WPR.Repository;
using System.Net;
using WPR.Services;
using WPR.Controllers.General.Cookie;
using WPR.Repository.DatabaseCheckRepository;
using WPR.Email;

namespace WPR;

/// <summary>
/// Deze class is responsible voor het configureren van de application services en database connectie
/// Het heeft methodes om de database te initializeren en het configureren van de web applicatie.
/// </summary>
public class AppConfigure
{
    public static void InitDatabase(IServiceProvider services)
    {
        var dbConnector = services.GetRequiredService<IConnector>();
        
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

    var cookiePolicyOptions = new CookiePolicyOptions
    {
        MinimumSameSitePolicy = SameSiteMode.Strict
    };


    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowSpecificOrigins", policy =>
            policy.WithOrigins("http://95.99.30.110:8080", "http://localhost:5173", "http://www.carandall.nl:8080")
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials()
        );
    });

    var urls = Environment.GetEnvironmentVariable("ASPNETCORE_URLS") ?? "http://0.0.0.0:80"; // Default to port 80 if not set

    Uri uri;
    try
    {
        uri = new Uri(urls);
    }
    catch (UriFormatException ex)
    {
        Console.WriteLine($"Invalid ASPNETCORE_URLS format: {urls}. Using default URL http://0.0.0.0:80");
        uri = new Uri("http://0.0.0.0:80");
    }

    builder.WebHost.ConfigureKestrel(options =>
    {
        // Ensure IP address is valid before binding
        if (uri.Host == "0.0.0.0" )
        {
            options.Listen(IPAddress.Any, 5000);  
        }
        else
        {
            try
            {
                options.Listen(IPAddress.Parse(uri.Host), uri.Port); 
            }
            catch (FormatException ex)
            {
                Console.WriteLine($"Invalid IP address specified: {uri.Host}. Using default binding to all IPs.");
                options.Listen(IPAddress.Any, uri.Port);  
            }
        }
    });
    
    // Register services for Dependency Injection
    builder.Services.AddSingleton<EnvConfig>(); // Singleton for environment configuration
    builder.Services.AddTransient<IConnector, Connector>(); // Transient for database connection.
    builder.Services.AddScoped<VehicleRepository>();
    builder.Services.AddScoped<IUserRepository, UserRepository>(); // Scoped for user repository
    builder.Services.AddScoped<IVehicleRepository, VehicleRepository>(); // Scoped for Vehicle Repository
    builder.Services.AddScoped<SessionHandler>(); // Scoped session handler
    builder.Services.AddScoped<Crypt>();
    builder.Services.AddScoped<Hashing.Hash>();
    builder.Services.AddScoped<EmailService>();
    builder.Services.AddScoped<IEmailService, EmailService>();
    builder.Services.AddScoped<IEmployeeRepository, EmployeeRepository>();
    builder.Services.AddScoped<IBackOfficeRepository, BackOfficeRepository>();
    builder.Services.AddScoped<IDatabaseCheckRepository, DatabaseCheckRepository>();
    builder.Services.AddScoped<IContractRepository, ContractRepository>();

    builder.Services.AddSingleton<IHostedService, Reminders>(); // Register Reminders as a singleton hosted service

    // Scoped dependencies
    builder.Services.AddScoped<EmailService>();
    builder.Services.AddScoped<Customer>();
    builder.Services.AddScoped<Vehicle>();
    builder.Services.AddScoped<Contract>();
    builder.Services.AddScoped<ICustomerDetails, Customer>();
    builder.Services.AddScoped<IVehicleDetails, Vehicle>();
    builder.Services.AddScoped<IContractDetails, Contract>();


    builder.Services.AddHostedService<Reminders>();

    

    // Configure authentication with cookie-based authentication schema.
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

    // Services for Swagger API
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    var app = builder.Build();

    app.UseSwagger();
    app.UseSwaggerUI(c => {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
        c.RoutePrefix = string.Empty;
    });
    app.UseCors("AllowSpecificOrigins");
    app.UseHttpsRedirection();
    app.MapControllers();
    app.UseAuthorization();
    app.UseAuthentication();
    app.UseCookiePolicy(cookiePolicyOptions);
    app.MapFallbackToFile("../wwwroot/index.html");

    //app.MapRazorPages();
    app.MapDefaultControllerRoute();

    return app;
    }



}
