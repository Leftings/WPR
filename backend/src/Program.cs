using System.Data;

namespace WPR;
using WPR.Data;
using WPR.Database;

public class Program
{
    public static void Main(string[] args)
    {
        var envConfig = new EnvConfig();
        var dbConnector = new Connector(envConfig);

        try
        {
            using (IDbConnection connection = dbConnector.DbConnect())
            {
                connection.Open();
                Console.WriteLine("Connection established");
            }
        }
        catch (Exception e)
        {
            Console.WriteLine("Failed to establish connection");
        }
    }
}