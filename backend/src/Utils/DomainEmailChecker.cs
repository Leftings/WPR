using System.Reflection.Metadata.Ecma335;
using MySql.Data.MySqlClient;
using WPR.Database;

namespace WPR.Utils;

public class DomainEmailChecker
{
    private readonly Connector _connector;
    public DomainEmailChecker(Connector connector)
    {
        _connector = connector ?? throw new ArgumentNullException(nameof(connector));
    }
    public async Task<(bool Found, int KvK)> DomainExists(string email)
    {
        string domain = $"@{email.Split("@").Last()}";
        Console.WriteLine(domain);
        string query = "SELECT Domain, KvK FROM Business";

        using (var connection = _connector.CreateDbConnection())
        using (var command = new MySqlCommand(query, (MySqlConnection)connection))
        using (var reader = await command.ExecuteReaderAsync())
        {
            while (await reader.ReadAsync())
            {
                if (((string)reader.GetValue(0)).Equals(domain))
                {
                    return (true, Convert.ToInt32(reader.GetValue(1)));
                }
            }

            return (false, 0);
        }

    }
}