using System.Data;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using WPR.Controllers.Cookie;
using WPR.Database;
using Microsoft.AspNetCore.Http.HttpResults;
using WPR.Utils;
using WPR.Hashing;
using Org.BouncyCastle.Crypto.Prng;
using WPR.Cryption;

namespace WPR.Repository;

/// <summary>
/// De UserRepository class geeft de methodes voor de interactie met de database voor gebruiker gerelateerde operaties.
/// Het implementeert de IUserRepository interface.
/// </summary>
public class UserRepository : IUserRepository
{
    private readonly Connector _connector;
    private readonly Hash _hash;
    private readonly Crypt _crypt;

    public UserRepository(Connector connector, Hash hash, Crypt crypt)
    {
        _connector = connector ?? throw new ArgumentNullException(nameof(connector));
        _hash = hash ?? throw new ArgumentNullException(nameof(hash));
        _crypt = crypt ?? throw new ArgumentNullException(nameof(crypt));
    }

    /*private async Task<bool> CheckPassword(string username, string password, string table)
    {
        string query = $@"SELECT password FROM {table} WHERE LOWER(email) = LOWER(@Email)";

        using (var connection = _connector.CreateDbConnection())
        
        using (var command = new MySqlCommand(query, (MySqlConnection)connection))
        {
            command.Parameters.AddWithValue("@Email", username);

            using (var reader = await command.ExecuteReaderAsync())
            {
                if (reader.HasRows)
                {
                    string passwordUser = reader.GetString("Password");

                    return _hash.(password, passwordUser);
                }

                return false;
            }
        }
    }
    */

    public async Task<bool> ValidateUserAsync(string username, string password, bool isEmployee)
    {
        string table = isEmployee ? "Staff" : "UserCustomer";
        /*string query = $@"SELECT 1 FROM {table} WHERE LOWER(email) = LOWER(@Email) AND BINARY password = @Password";

        using (var connection = _connector.CreateDbConnection())
        {
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
        */

        string query = $@"SELECT password FROM {table} WHERE LOWER(email) = LOWER(@Email)";

        using (var connection = _connector.CreateDbConnection())
        
        using (var command = new MySqlCommand(query, (MySqlConnection)connection))
        {
            command.Parameters.AddWithValue("@Email", username);

            using (var reader = await command.ExecuteReaderAsync())
            {
                if (await reader.ReadAsync())
                {
                    string passwordUser = reader.GetString("Password");
                    return _hash.createHash(password).Equals(passwordUser);
                }

                return false;
            }
        }
    }

    public async Task<(bool status, string message)> checkUsageEmailAsync(string email)
    {
        try
        {
            string query = "SELECT COUNT(*) FROM UserCustomer WHERE LOWER(Email) = LOWER(@E)";

            using (var connection = _connector.CreateDbConnection())
            using (var command = new MySqlCommand(query, (MySqlConnection)connection))
            {
                command.Parameters.AddWithValue("@E", email);
                bool inUse = Convert.ToInt32(await command.ExecuteScalarAsync()) > 0;

                Console.WriteLine(inUse);
                return (inUse, inUse ? "Email detected" : "No email detected");
            }
        }

        catch (MySqlException ex)
        {
            await Console.Error.WriteLineAsync($"Database error: {ex.Message}");
            return (false, ex.Message);
        }
        catch (Exception ex)
        {
            await Console.Error.WriteLineAsync($"Unexpected error: {ex.Message}");
            return (false, ex.Message);
        }
    }

    public async Task<(bool status, string message, int newUserID)> addCustomerAsync(Object[] personData)
    {
        try
        {
            string query = "INSERT INTO UserCustomer (Adres, Telnum, Password, Email, FirstName, LastName) values (@A, @T, @P, @E, @F, @L)";

            using (var connection = _connector.CreateDbConnection())
            using (var command = new MySqlCommand(query, (MySqlConnection)connection))
            {
                command.Parameters.AddWithValue("@A", personData[0]);
                command.Parameters.AddWithValue("@T", personData[1]);
                command.Parameters.AddWithValue("@P", personData[2]);
                command.Parameters.AddWithValue("@E", personData[3]);
                command.Parameters.AddWithValue("@F", personData[4]);
                command.Parameters.AddWithValue("@L", personData[5]);

                if (await command.ExecuteNonQueryAsync() > 0)
                {
                    command.CommandText = "SELECT LAST_INSERT_ID();";
                    int newUserID = Convert.ToInt32(await command.ExecuteScalarAsync());

                    return (true, "Data Inserted", newUserID);
                }
                return (false, "No Data Inserted", -1);
            }
        }

        catch (MySqlException ex)
        {
            await Console.Error.WriteLineAsync($"Database error: {ex.Message}");
            return (false, ex.Message, -1);
        }
        catch (Exception ex)
        {
            await Console.Error.WriteLineAsync($"Unexpected error: {ex.Message}");
            return (false, ex.Message, -1);
        }
    }

    public async Task<(bool status, string message)> addPersonalCustomerAsync(Object[] personalData)
    {
        try
        {
            string query = "INSERT INTO UserPersonal (ID, BirthDate) values (@I, @B)";

            using (var connection = _connector.CreateDbConnection())
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
            await Console.Error.WriteLineAsync($"Database error: {ex.Message}");
            return (false, ex.Message);
        }
        catch (Exception ex)
        {
            await Console.Error.WriteLineAsync($"Unexpected error: {ex.Message}");
            return (false, ex.Message);
        }
    }

    public async Task<(bool status, string message)> addEmployeeCustomerAsync(Object[] employeeData)
    {
        try
        {
            string query = "INSERT INTO UserEmployee (ID, Business) values (@I, @B)";

            using (var connection = _connector.CreateDbConnection())
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
            await Console.Error.WriteLineAsync($"Database error: {ex.Message}");
            return (false, ex.Message);
        }
        catch (Exception ex)
        {
            await Console.Error.WriteLineAsync($"Unexpected error: {ex.Message}");
            return (false, ex.Message);
        }
    }

    public async Task<int> GetUserIdAsync(string email)
    {
        try
        {
            string query = "SELECT ID FROM UserCustomer WHERE Email = @E";

            using (var connection = _connector.CreateDbConnection())
            using (var command = new MySqlCommand(query, (MySqlConnection)connection))
            {
                command.Parameters.AddWithValue("@E", email);

                var result = await command.ExecuteScalarAsync();

                if (result != null)
                {
                    return Convert.ToInt32(result);
                }

                return -1;
            }
        }
        catch (Exception ex)
        {
            await Console.Error.WriteLineAsync($"Unexpected error: {ex.Message}");
            return -1;
        }
    }

    public async Task<string> GetUserNameAsync(string userId)
    {
        try
        {
            string query = "SELECT FirstName FROM UserCustomer WHERE ID = @I";

            using (var connection = _connector.CreateDbConnection())
            using (var command = new MySqlCommand(query, (MySqlConnection)connection))
            {
                Console.WriteLine(userId);
                command.Parameters.AddWithValue("@I", Convert.ToInt32(userId));

                var result = await command.ExecuteScalarAsync();

                return result.ToString();
            }
        }
        catch (Exception ex)
        {
            await Console.Error.WriteLineAsync($"Unexpected error: {ex.Message}");
            return ex.ToString();
        }
    }

    public async Task<(bool status, string message)> EditUserInfoAsync(List<object[]> data)
    {
        try
        {
            var query = await CreateUserInfoQuery(data);

            if (query.goodQuery)
            {
                using (var connection = _connector.CreateDbConnection())
                using (var command = new MySqlCommand(query.message, (MySqlConnection)connection))
                {
                    var result = await command.ExecuteNonQueryAsync();

                    Console.WriteLine(result);
                    Console.WriteLine(query);

                    if (await command.ExecuteNonQueryAsync() > 0)
                    {
                        return (true, "Data inserted");
                    }
                    
                    return (false, "Data not inserted");
                }
            }
            else
            {
                return (false, query.message);
            }
        }
        catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex}");
                return (false, ex.ToString());
            }
    }

    public async Task<bool> IsKvkNumberAsync(int kvkNumber)
    {
        try
        {
            string query = "SELECT COUNT(1) FROM Business WHERE KVK = @kvkNumber";

            using (var connection = _connector.CreateDbConnection())
            using (var command = new MySqlCommand(query, (MySqlConnection) connection))
            {
                command.Parameters.AddWithValue("@kvkNumber", kvkNumber);
                var result = Convert.ToInt32(await command.ExecuteScalarAsync());
                return result > 0;
            }
        }
        catch (MySqlException ex)
        {
            await Console.Error.WriteLineAsync($"Database error: {ex.Message}");
            return false;
        }
        catch (Exception ex)
        {
            await Console.Error.WriteLineAsync($"Unexpected error: {ex.Message}");
            return false;
        }
    }

    public async Task<(bool status, string message)> DeleteUserAsync(string userId)
    {
        Console.WriteLine("TEST");
        try
        {
            string queryCustomer = "DELETE FROM UserCustomer WHERE ID = @ID";
            string queryEmployee = "DELETE FROM UserEmployee WHERE ID = @ID";
            string queryPersonal = "DELETE FROM UserPersonal WHERE ID = @ID";

            using (var connection = _connector.CreateDbConnection())
            using (var customerCommand = new MySqlCommand(queryCustomer, (MySqlConnection)connection))
            using (var employeeCommand = new MySqlCommand(queryEmployee, (MySqlConnection)connection))
            using (var personalCommand = new MySqlCommand(queryPersonal, (MySqlConnection)connection))

            {
                Console.WriteLine(userId);
                int userIdInt = Convert.ToInt32(userId);
                Console.WriteLine(userIdInt);

                customerCommand.Parameters.AddWithValue("@ID", userIdInt);
                employeeCommand.Parameters.AddWithValue("@ID", userIdInt);
                personalCommand.Parameters.AddWithValue("@ID", userIdInt);

                await employeeCommand.ExecuteNonQueryAsync();
                await personalCommand.ExecuteNonQueryAsync();
                
                int rowsAffected = await customerCommand.ExecuteNonQueryAsync();

                if (rowsAffected > 0)
                {
                    return (true, "User deleted");
                }
                else
                {
                    return (false, "User could not be deleted");
                }
            }
        }
        catch (MySqlException ex)
        {
            // Handle database errors
            await Console.Error.WriteLineAsync($"Database error: {ex.Message}");
            return (false, "Database error: " + ex.Message);
        }
        catch (Exception ex)
        {
            // Handle other errors
            await Console.Error.WriteLineAsync($"Unexpected error: {ex.Message}");
            return (false, "Unexpected error: " + ex.Message);
        }
    }


    private async Task<(bool goodQuery, string message)> CreateUserInfoQuery(List<object[]> data)
    {
        int lengthList = data.Count();
        string query = "UPDATE UserCustomer SET ";

        for (int i = 1; i < lengthList; i++)
        {
            object[] item = data[i];

            if (item[2].Equals("System.Int32"))
            {
                query += $"{item[0]} = {item[1]}";
            }
            else
            {
                if (item[0].ToString().Equals("Email"))
                {
                    var emailCheck = await checkUsageEmailAsync(item[1].ToString());

                    if (emailCheck.status)
                    {
                        return (false, emailCheck.message);
                    }
                }
                query += $"{item[0]} = '{item[1]}'";
            }

            if (i + 1 != lengthList)
            {
                query += ",";
            }

            query += " ";
        }

        return (true, query += $"WHERE ID = {data[0][1]}");
    }
    
}