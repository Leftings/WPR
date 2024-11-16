namespace WPR.login;
using WPR.Database;
using WPR.Data;
using MySql.Data.MySqlClient;

public class Check
{
    private Connector _connector { get; set; }
    public Check()
    {
        _connector = new Connector(new EnvConfig());
    }

    public bool SuccessfulInlog(string email, string password)
    {
        using(var connection = _connector.CreateDbConnection())
        {
            string query = $"SELECT * FROM Staff WHERE username = {email} AND password = {password}";

            using (var command = new MySqlCommand(query, (MySqlConnection)connection))
            {
                return command.ExecuteReader().HasRows;
            }
        }
    }
}