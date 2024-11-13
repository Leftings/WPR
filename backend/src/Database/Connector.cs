using System.Data;
using MySql.Data.MySqlClient;
using Environment = WPR.Data.Environment;

namespace WPR.Database;

public class Connector
{
    private readonly Environment _environment;

    public Connector(Environment environment)
    {
        _environment = environment;
    }

    public IDbConnection DbConnect()
    {
        if (!_environment.IsConfigured())
        {
            throw new InvalidOperationException("Not configured");
        }
        
        string server = _environment.Get("DB_SERVER");
        string database = _environment.Get("DB_DATABASE");
        string username = _environment.Get("DB_USERNAME");
        string password = _environment.Get("DB_PASSWORD");
        
        string connectString = $"Server={server};Database={database};User ID={username};Password={password};";
        
        return new MySqlConnection(connectString);
    }
}