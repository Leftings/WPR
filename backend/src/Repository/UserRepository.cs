using System.Data;
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

    public async Task<(bool status, string message)> checkUsageEmailAsync(IDbConnection connection, string email)
    {
        try
        {
            string query = "SELECT COUNT(*) FROM User_Customer WHERE LOWER(Email) = LOWER(@E)";

            using (var command = new MySqlCommand(query, (MySqlConnection)connection))
            {
                command.Parameters.AddWithValue("@E", email);
                bool inUse = Convert.ToInt32(await command.ExecuteScalarAsync()) > 0;

                return (inUse, inUse ? "No email detected" : "Email detected");
            }
        }

        catch (MySqlException ex)
        {
            Console.Error.WriteLine($"Database error: {ex.Message}");
            return (false, ex.Message);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Unexpected error: {ex.Message}");
            return (false, ex.Message);
        }
    }

    public async Task<(bool status, string message, int newUserID)> addCustomerAsync(IDbConnection connection, Object[] personData)
    {
        try
        {
            string query = "INSERT INTO User_Customer (Adres, Telnum, Password, Email) values (@A, @T, @P, @E)";

            using (var command = new MySqlCommand(query, (MySqlConnection)connection))
            {
                command.Parameters.AddWithValue("@A", personData[0]);
                command.Parameters.AddWithValue("@T", personData[1]);
                command.Parameters.AddWithValue("@P", personData[2]);
                command.Parameters.AddWithValue("@E", personData[3]);

                if (await command.ExecuteNonQueryAsync() > 0)
                {
                    command.CommandText = "SELECT LAST_INSERT_ID();";
                    int newUserID = Convert.ToInt32(command.ExecuteScalar());

                    return (true, "Data Inserted", newUserID);
                }
                return (false, "No Data Inserted", -1);
            }
        }

        catch (MySqlException ex)
        {
            Console.Error.WriteLine($"Database error: {ex.Message}");
            return (false, ex.Message, -1);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Unexpected error: {ex.Message}");
            return (false, ex.Message, -1);
        }
    }

    public async Task<(bool status, string message)> addPersonalCustomerAsync(IDbConnection connection, Object[] personalData)
    {
        try
        {
            string query = "INSERT INTO Personal (ID, BirthDate) values (@I, @B)";

            using (var command = new MySqlCommand(query, (MySqlConnection)connection))
            {
                command.Parameters.AddWithValue("@I", personalData[0]);
                command.Parameters.AddWithValue("@B", personalData[1]);

                if (await command.ExecuteNonQueryAsync() > 0)
                {
                    return (true, "Data Inserted");
                }
                return (false, "No Data Inserted");
            }
        }

        catch (MySqlException ex)
        {
            Console.Error.WriteLine($"Database error: {ex.Message}");
            return (false, ex.Message);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Unexpected error: {ex.Message}");
            return (false, ex.Message);
        }
    }

    public async Task<(bool status, string message)> addEmployeeCustomerAsync(IDbConnection connection, Object[] employeeData)
    {
        try
        {
            string query = "INSERT INTO Employee (ID, Business) values (@I, @B)";

            using (var command = new MySqlCommand(query, (MySqlConnection)connection))
            {
                command.Parameters.AddWithValue("@I", employeeData[0]);
                command.Parameters.AddWithValue("@B", employeeData[1]);

                if (await command.ExecuteNonQueryAsync() > 0)
                {
                    return (true, "Data Inserted");
                }
                return (false, "No Data Inserted");
            }
        }

        catch (MySqlException ex)
        {
            Console.Error.WriteLine($"Database error: {ex.Message}");
            return (false, ex.Message);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Unexpected error: {ex.Message}");
            return (false, ex.Message);
        }
    }
}