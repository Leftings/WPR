using System.Data;
using MySql.Data.MySqlClient;
using WPR.Data;

namespace WPR.Database;

/// <summary>
/// Connector class om verbinding te maken met de database,
/// maakt gebruik van de connectie strings die zijn uitgelezen in EnvConfig.
/// </summary>

public class Connector
{
    private readonly EnvConfig _envConfig; // Instantie van EnConfig om environment variabelen op te halen.

    public Connector(EnvConfig envConfig)
    {
        _envConfig = envConfig;
    }
    
    /// <summary>
    /// Create en open de database connectie door het gebruiken van de configuratie values.
    /// </summary>
    /// <returns>Een geopende IDbConnection instantie</returns>
    public IDbConnection CreateDbConnection()
    {
        if (!_envConfig.IsConfigured()) // Check of alle environment variabelen zijn geconfigureerd
        {
            throw new InvalidOperationException("Not configured");
        }
        
        // Haal connectie strings op van de environment configuratie
        string server = _envConfig.Get("DB_SERVER");
        string database = _envConfig.Get("DB_DATABASE");
        string username = _envConfig.Get("DB_USERNAME");
        string password = _envConfig.Get("DB_PASSWORD");
        
        // Connectie string met de opgehaalde parameters
        string connectString = $"Server={server};Database={database};User ID={username};Password={password};";

        try
        {
            var connection = new MySqlConnection(connectString); // Maak een nieuwe MySQL connectie instantie
            connection.Open();
            Console.WriteLine("Database connection successful");
            return connection; // return de open connectie
        }
        catch (MySqlException ex)
        {
            Console.WriteLine($"Error connecting to database: {ex.Message}");
            throw; 
        }
    }
}