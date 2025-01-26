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

public class AppConfigure
{
    /// <summary>
    /// Initialiseert de databaseverbinding en controleert of de verbinding succesvol tot stand is gekomen.
    /// </summary>
    public static void InitDatabase(IServiceProvider services)
    {
        var dbConnector = services.GetRequiredService<Connector>();
        
        try
        {
            using (var connection = dbConnector.CreateDbConnection())
            {
                Console.WriteLine("Database connection established"); // Succesvolle verbinding
            }
        }
        catch (Exception e)
        {
            Console.WriteLine("Failed to establish database connection"); // Foutmelding bij mislukte verbinding
            Console.WriteLine(e.StackTrace); // Toon de stacktrace van de fout
        }
    }

    /// <summary>
    /// Configureert de web applicatie, inclusief middleware, services, authenticatie en CORS-instellingen.
    /// </summary>
    /// <param name="args">De command line argumenten bij het opstarten van de applicatie.</param>
    /// <returns>De geconfigureerde WebApplication instantie.</returns>
    public static WebApplication ConfigureApplication(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args); // Maak een builder aan voor de webapplicatie

        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "development"; // Verkrijg de omgeving (development of production)
        builder.Configuration.AddEnvironmentVariables(); // Voeg omgevingsvariabelen toe aan de configuratie

        // Afhankelijk van de omgeving, laad het juiste configuratiebestand
        if (environment == "development")
        {
            builder.Configuration.AddJsonFile(".env", optional: true, reloadOnChange: true); // Configuratie voor development
        }
        else if (environment == "Production")
        {
            builder.Configuration.AddJsonFile(".env.deployment", optional: true, reloadOnChange: true); // Configuratie voor productie
        }

        // CORS-instellingen om specifieke domeinen toe te staan
        var cookiePolicyOptions = new CookiePolicyOptions
        {
            MinimumSameSitePolicy = SameSiteMode.Strict // Zorg ervoor dat cookies alleen met dezelfde site worden gedeeld
        };

        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowSpecificOrigins", policy =>
                policy.WithOrigins("http://95.99.30.110:8080", "http://localhost:5173", "http://www.carandall.nl:8080") // Voeg specifieke toegestane domeinen toe
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials() // Sta cookies toe
            );
        });

        // Configureer de URL waarop de applicatie wordt uitgevoerd
        var urls = Environment.GetEnvironmentVariable("ASPNETCORE_URLS") ?? "http://0.0.0.0:80"; // Standaard poort 80
        Uri uri;
        try
        {
            uri = new Uri(urls); // Parse de URL
        }
        catch (UriFormatException ex)
        {
            Console.WriteLine($"Invalid ASPNETCORE_URLS format: {urls}. Using default URL http://0.0.0.0:80");
            uri = new Uri("http://0.0.0.0:80"); // Foutafhandelingslogica voor ongeldige URL
        }

        // Configureer Kestrel om te luisteren naar de juiste host en poort
        builder.WebHost.ConfigureKestrel(options =>
        {
            if (uri.Host == "0.0.0.0")
            {
                options.Listen(IPAddress.Any, 5000); // Luister naar elke IP voor poort 5000
            }
            else
            {
                try
                {
                    options.Listen(IPAddress.Parse(uri.Host), uri.Port); // Luister naar opgegeven host en poort
                }
                catch (FormatException ex)
                {
                    Console.WriteLine($"Invalid IP address specified: {uri.Host}. Using default binding to all IPs.");
                    options.Listen(IPAddress.Any, uri.Port); // Foutafhandelingslogica voor ongeldige IP-adressen
                }
            }
        });

        // Registratie van services in Dependency Injection container
        builder.Services.AddSingleton<EnvConfig>(); // Singleton voor omgevingsconfiguratie
        builder.Services.AddTransient<Connector>(); // Transient voor databaseverbinding
        builder.Services.AddScoped<VehicleRepository>(); // Scoped voor repository voor voertuigen
        builder.Services.AddScoped<IUserRepository, UserRepository>(); // Scoped voor gebruikersrepository
        builder.Services.AddScoped<IVehicleRepository, VehicleRepository>(); // Scoped voor voertuigrepository
        builder.Services.AddScoped<SessionHandler>(); // Scoped voor sessiebeheer
        builder.Services.AddScoped<Crypt>(); // Scoped voor cryptografie
        builder.Services.AddScoped<Hashing.Hash>(); // Scoped voor hashing
        builder.Services.AddScoped<EmailService>(); // Scoped voor emailservice
        builder.Services.AddScoped<IEmployeeRepository, EmployeeRepository>(); // Scoped voor medewerkersrepository
        builder.Services.AddScoped<IBackOfficeRepository, BackOfficeRepository>(); // Scoped voor backoffice repository
        builder.Services.AddScoped<IDatabaseCheckRepository, DatabaseCheckRepository>(); // Scoped voor database check repository
        builder.Services.AddScoped<IContractRepository, ContractRepository>(); // Scoped voor contractrepository

        builder.Services.AddSingleton<IHostedService, Reminders>(); // Singleton voor Reminders service

        // Scoped dependencies voor klant-, voertuig- en contractdetails
        builder.Services.AddScoped<EmailService>();
        builder.Services.AddScoped<IDetails, Customer>();
        builder.Services.AddScoped<IDetails, Vehicle>();
        builder.Services.AddScoped<IDetails, Contract>();
        builder.Services.AddScoped<Customer>();
        builder.Services.AddScoped<Vehicle>();
        builder.Services.AddScoped<Contract>();

        // Hosted service voor herinneringen
        builder.Services.AddHostedService<Reminders>();

        // Configuratie van cookie-gebaseerde authenticatie
        builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(options =>
            {
                options.ExpireTimeSpan = TimeSpan.FromMinutes(5); // Verlooptijd van cookies
                options.SlidingExpiration = true; // Herstellen van de vervaldatum bij activiteit
                options.AccessDeniedPath = "/Forbidden/"; // Pad voor toegang geweigerd
            });

        // Configuratie van controllers en JSON-instellingen
        builder.Services.AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles; // Cyclusverwijzingen negeren bij serialisatie
            });

        // Services voor Swagger API-documentatie
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        var app = builder.Build(); // Maak de WebApplication aan

        // Configureer Swagger UI en CORS
        app.UseSwagger();
        app.UseSwaggerUI(c => {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
            c.RoutePrefix = string.Empty; // Verwijder standaard pad voor Swagger UI
        });

        app.UseCors("AllowSpecificOrigins"); // Gebruik de eerder gedefinieerde CORS policy
        app.UseHttpsRedirection(); // Forceer HTTPS
        app.MapControllers(); // Map de controllers
        app.UseAuthorization(); // Schakel autorisatie in
        app.UseAuthentication(); // Schakel authenticatie in
        app.UseCookiePolicy(cookiePolicyOptions); // Gebruik de cookie policy

        // Standaard controller route
        app.MapDefaultControllerRoute();

        return app; // Retourneer de geconfigureerde WebApplication
    }
}
