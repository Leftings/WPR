using MySql.Data.MySqlClient;
using WPR.Database;

namespace WPR.Repository;

public class UserRepository : IUserRepository
{
    private readonly Connector _connector;

    public UserRepository(Connector connector)
    {
        _connector = connector ?? throw new ArgumentNullException(nameof(connector));
    }

    public async Task<bool> ValidateUserAsync(string username, string password, bool isEmployee)
    {
        string table = isEmployee ? "Staff" : "User_Customer";
        string query = $@"SELECT 1 FROM {table} WHERE LOWER(email) = LOWER(@Email) AND BINARY password = @Password";

        using (var connection = _connector.CreateDbConnection())
        using (var command = new MySqlCommand(query, (MySqlConnection)connection))
        {
            command.Parameters.AddWithValue("@Email", username);
            command.Parameters.AddWithValue("@Password", password);

            using (var reader = await command.ExecuteReaderAsync())
            {
                return reader.HasRows;
            }
        }
    }
}