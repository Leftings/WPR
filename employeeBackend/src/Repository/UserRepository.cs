using Employee.Database;
using Employee.Hashing;
using MySql.Data.MySqlClient;

namespace Employee.Repository;

/// <summary>
/// De UserRepository class geeft de methodes voor de interactie met de database voor gebruiker gerelateerde operaties.
/// Het implementeert de IUserRepository interface.
/// </summary>
public class UserRepository : IUserRepository
{
    private readonly Connector _connector;
    private readonly Hash _hash;

    public UserRepository(Connector connector, Hash hash)
    {
        _connector = connector ?? throw new ArgumentNullException(nameof(connector));
        _hash = hash ?? throw new ArgumentNullException(nameof (hash));
    }
    
    // vehicleBlob is het pad naar de afbeelding
    public async Task<(bool status, string message)> AddVehicleAsync(int yop, string brand, string type, string licensePlate, string color, string sort, double price, string description, byte[] vehicleBlob)
    {
        try
        {
            string query = "INSERT INTO Vehicle (YoP, Brand, Type, LicensePlate, Color, Sort, Price, Description, Vehicleblob) VALUES (@Y, @B, @T, @L, @C, @S, @P, @D, @V)";

            using (var connection = _connector.CreateDbConnection())
            using (var command = new MySqlCommand(query, (MySqlConnection)connection))
            {
                command.Parameters.AddWithValue("@Y", yop);
                command.Parameters.AddWithValue("@B", brand);
                command.Parameters.AddWithValue("@T", type);
                command.Parameters.AddWithValue("@L", licensePlate);
                command.Parameters.AddWithValue("@C", color);
                command.Parameters.AddWithValue("@S", sort);
                command.Parameters.AddWithValue("@P", price);
                command.Parameters.AddWithValue("@D", description);
                command.Parameters.AddWithValue("@V", vehicleBlob);

                if (await command.ExecuteNonQueryAsync() > 0)
                {
                    return (true, "Vehicle inserted");
                }
                return (false, "Something went wrong while inserting the vehicle");
            }
        }
        catch (MySqlException ex)
        {
            return (false, ex.Message);
        }
    }

    public async Task<(bool status, string message)> AddStaff(Object[] personData)
    {
        try
        {
            string query = "INSERT INTO Staff (FirstName, LastName, Password, Email, Office) VALUES (@F, @L, @P, @E, @O)";

            using (var connection = _connector.CreateDbConnection())
            using (var command = new MySqlCommand(query, (MySqlConnection)connection))
            {
                command.Parameters.AddWithValue("@F", personData[0]);
                command.Parameters.AddWithValue("@L", personData[1]);
                command.Parameters.AddWithValue("@P", _hash.createHash(personData[2].ToString()));
                command.Parameters.AddWithValue("@E", personData[3]);
                command.Parameters.AddWithValue("@O", personData[4]);

                if (await command.ExecuteNonQueryAsync() > 0)
                    {
                        return (true, "Data inserted");
                    }
                    
                    return (false, "Data not inserted");
            }
        }
        catch(MySqlException ex)
        {
            return (false, ex.ToString());
        }
    }

    public async Task<(bool status, string message)> checkUsageEmailAsync(string email)
    {
        try
        {
            string query = "SELECT COUNT(*) FROM Staff WHERE LOWER(Email) = LOWER(@E)";

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

    public async Task<(bool status, List<Dictionary<string, object>> data)> GetInReviewFromUserAsync(string id)
    {
        try
        {
            string query = "SELECT * FROM Abonnement WHERE ReviewedBy = @I";

            using (var connection = _connector.CreateDbConnection())
            using (var command = new MySqlCommand(query, (MySqlConnection)connection))
            {
                command.Parameters.AddWithValue("@I", id);

                using (var reader = await command.ExecuteReaderAsync())
                {
                    var result = new List<Dictionary<string, object>>();

                    while (await reader.ReadAsync())
                    {
                        var row = new Dictionary<string, object>();

                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            row[reader.GetName(i)] = reader.GetValue(i);
                        }

                        result.Add(row);
                    }

                    return (true, result);
                }
            }
        }
        catch (MySqlException ex)
        {
            await Console.Error.WriteLineAsync($"Database error: {ex.Message}");
            return (false, null);
        }
        catch (Exception ex)
        {
            await Console.Error.WriteLineAsync($"Unexpected error: {ex.Message}");
            return (false, null);
        }
    }

    public async Task<(bool status, List<Dictionary<string, object>> data)> GetRequestedAsync()
    {
        try
        {
            string query = "SELECT * FROM Abonnement WHERE Status = 'requested'";

            using (var connection = _connector.CreateDbConnection())
            using (var command = new MySqlCommand(query, (MySqlConnection)connection))
            {
                using (var reader = await command.ExecuteReaderAsync())
                {
                    var result = new List<Dictionary<string, object>>();

                    while (await reader.ReadAsync())
                    {
                        var row = new Dictionary<string, object>();

                        var getUserData = GetUserDataAsync(reader.GetValue(5).ToString());
                        var getVehiceData = GetVehicleData(reader.GetValue(4).ToString());

                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            row[reader.GetName(i)] = reader.GetValue(i);
                        }

                        var userData = await getUserData;

                        foreach (var userField in userData.data)
                        {
                            row[userField.Key] = userField.Value;
                        }

                        var vehicleData = await getVehiceData;

                        foreach (var vehicelField in vehicleData.data)
                        {
                            row[vehicelField.Key] = vehicelField.Value;
                        }
                        
                        result.Add(row);
                    }

                    return (true, result);
                }
            }
        }
        catch (MySqlException ex)
        {
            await Console.Error.WriteLineAsync($"Database error: {ex.Message}");
            return (false, null);
        }
        catch (Exception ex)
        {
            await Console.Error.WriteLineAsync($"Unexpected error: {ex.Message}");
            return (false, null);
        }
    }

    private async Task<(bool status, Dictionary<string, object> data)> GetUserDataAsync(string userid)
    {
        try
        {
            string query = "SELECT FirstName, LastName, Adres, Email, TelNum FROM UserCustomer WHERE ID = @I";

            using (var connection = _connector.CreateDbConnection())
            using (var command = new MySqlCommand(query, (MySqlConnection)connection))
            {
                command.Parameters.AddWithValue("@I", userid);

                using (var reader = await command.ExecuteReaderAsync())
                {
                    await reader.ReadAsync();
                    
                    var row = new Dictionary<string, object>();

                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        row[reader.GetName(i)] = reader.GetValue(i);
                    }

                    return (true, row);
                }
            }
        }
        catch (MySqlException ex)
        {
            await Console.Error.WriteLineAsync($"Database error: {ex.Message}");
            return (false, null);
        }
        catch (Exception ex)
        {
            await Console.Error.WriteLineAsync($"Unexpected error: {ex.Message}");
            return (false, null);
        }
    }

    private async Task<(bool status, Dictionary<string, object> data)> GetVehicleData(string carId)
    {
        try
        {
            string query = "SELECT Brand, Type, LicensePlate FROM Vehicle WHERE FrameNr = @I";

            using (var connection = _connector.CreateDbConnection())
            using (var command = new MySqlCommand(query, (MySqlConnection)connection))
            {
                command.Parameters.AddWithValue("@I", carId);

                using (var reader = await command.ExecuteReaderAsync())
                {
                    await reader.ReadAsync();
                    
                    var row = new Dictionary<string, object>();

                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        row[reader.GetName(i)] = reader.GetValue(i);
                    }

                    return (true, row);
                }
            }
        }
        catch (MySqlException ex)
        {
            await Console.Error.WriteLineAsync($"Database error: {ex.Message}");
            return (false, null);
        }
        catch (Exception ex)
        {
            await Console.Error.WriteLineAsync($"Unexpected error: {ex.Message}");
            return (false, null);
        }
    }
}