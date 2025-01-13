using dotenv.net; // library om .env files te laden
using System;

namespace WPR.Data;

/// <summary>
/// Leez het .env bestand uit waarin de connectie strings staan naar de database.
/// Deze staan in een apart bestand buiten de git repo,
/// zodat mensen van buiten niet met de database kunnen verbinden.
/// </summary>

public class EnvConfig
{
    private readonly IConfiguration _configuration;
    
    public EnvConfig()
    {
        DotEnv.Load();  // Gebruikt de dotenv.net library om het bestand te laden

        var builder = new ConfigurationBuilder()
            .AddEnvironmentVariables(); // Voegt de environment variabelen aan de ConfigurationBuilder
        
        _configuration = builder.Build(); // Bouwt het configuratie object
    }

    /// <summary>
    /// Haalt de value van de specifieke environement variabelen.
    /// </summary>
    /// <param name="key">Ophalen environment variabele key.</param>
    /// <returns>De value van de specifieke environment variabele, of null als niet gevonden.</returns>
    public virtual string Get(string key)
    {
        var value = _configuration[key]; // Fetch de value van de gegeven key
        if (string.IsNullOrEmpty(value)) 
        {
            Console.WriteLine($"Warning {key} not found in environment");
        }
        return value;
    }

    /// <summary>
    /// Check om te kijken of alle environment variabelen geconfigureerd zijn.
    /// </summary>
    /// <returns>True als alle keys present zijn, anders False.</returns>
    public virtual bool IsConfigured()
    {
        var requiredKeys = new[] { "DB_SERVER", "DB_DATABASE", "DB_USERNAME", "DB_PASSWORD" };
        return requiredKeys.All(key => !string.IsNullOrEmpty(Get(key)));
    }
    
    public string GetSmtpHost() => Get("SMTP_HOST");
    public int GetSmtpPort() => int.TryParse(Get("SMTP_PORT"), out var port) ? port : 0;
    public string GetFromEmail() => Get("FROM_EMAIL");
    public string GetFromPassword() => Get("FROM_PASSWORD");
}