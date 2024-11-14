using System.Data;
using MySql.Data.MySqlClient;
using WPR.Data;

namespace WPR.Database;

public class Connector
{
    private readonly EnvConfig _envConfig;

    public Connector(EnvConfig envConfig)
    {
        _envConfig = envConfig;
    }

    public IDbConnection CreateDbConnection()
    {
        if (!_envConfig.IsConfigured())
        {
            throw new InvalidOperationException("Not configured");
        }
        
        string server = _envConfig.Get("DB_SERVER");
        string database = _envConfig.Get("DB_DATABASE");
        string username = _envConfig.Get("DB_USERNAME");
        string password = _envConfig.Get("DB_PASSWORD");
        
        string connectString = $"Server={server};Database={database};User ID={username};Password={password};";
        
        var connection = new MySqlConnection(connectString);
        connection.Open();
        return connection;
    }
}