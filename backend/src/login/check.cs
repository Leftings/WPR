namespace WPR.login;
using WPR.Database;
using WPR.Data;
using MySql.Data.MySqlClient;

public class Check
{
    private readonly Connector _connector;
    public Check()
    {
        _connector = new Connector(new EnvConfig());
    }

    public bool SuccessfullLogin(string email, string password)
    {
        try
        {
            using (var connection = _connector.CreateDbConnection())
            {
                string query = "SELECT 1 FROM Staff WHERE username= @Email AND password = @Password";

                using (var command = new MySqlCommand(query, (MySqlConnection)connection))
                {
                    command.Parameters.AddWithValue("@Email", email);
                    command.Parameters.AddWithValue("@Password", password);

                    using (var reader = command.ExecuteReader())
                    {
                        return reader.HasRows;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error with login: {ex.Message}");
            return false;
        }
    }
}