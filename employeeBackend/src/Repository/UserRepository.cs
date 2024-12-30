using Employee.Database;
using Employee.Hashing;
using Microsoft.VisualBasic;
using MySql.Data.MySqlClient;
using MySqlX.XDevAPI.Relational;

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

            // Er wordt een connectie met de DataBase gemaakt met de bovenstaande query
            using (var connection = _connector.CreateDbConnection())
            using (var command = new MySqlCommand(query, (MySqlConnection)connection))
            {
                // Alle parameters worden ingevuld
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
                    // Er wordt gekeken of de gegevens zijn ingevoerd in de DataBase
                    return (true, "Vehicle inserted");
                }
                return (false, "Something went wrong while inserting the vehicle");
            }
        }
        catch (MySqlException ex)
        {
            Console.WriteLine(ex.Message);
            return (false, ex.Message);
        }
    }

    public async Task<(bool status, string message)> AddStaff(Object[] personData)
    {
        try
        {
            // Er wordt een specifieke query aangewezen tussen VehicleManagers en Offices
            string query;

            if (personData[4].Equals("Wagen"))
            {
                query = "INSERT INTO VehicleManager (FirstName, LastName, Password, Email, Business) VALUES (@F, @L, @P, @E, @B)";
            }
            else
            {
                query = "INSERT INTO Staff (FirstName, LastName, Password, Email, Office) VALUES (@F, @L, @P, @E, @O)";
            }

            // Er wordt een connectie met de DataBase gemaakt met de bovenstaande query
            using (var connection = _connector.CreateDbConnection())
            using (var command = new MySqlCommand(query, (MySqlConnection)connection))
            {
                // Alle parameters worden ingevuld
                command.Parameters.AddWithValue("@F", personData[0]);
                command.Parameters.AddWithValue("@L", personData[1]);
                command.Parameters.AddWithValue("@P", _hash.createHash(personData[2].ToString()));
                command.Parameters.AddWithValue("@E", personData[3]);

                if (personData[4].Equals("Wagen"))
                {
                    command.Parameters.AddWithValue("@B", personData[5]);
                }
                else
                {
                    command.Parameters.AddWithValue("@O", personData[4]);
                }

                if (await command.ExecuteNonQueryAsync() > 0)
                    {
                        // Er wordt gekeken of de gegevens zijn ingevoerd in de DataBase
                        return (true, "Data inserted");
                    }
                    
                    return (false, "Data not inserted");
            }
        }
        catch(MySqlException ex)
        {
            Console.WriteLine(ex.Message);
            return (false, ex.Message);
        }
    }

    public async Task<(bool status, string message)> checkUsageEmailAsync(string email)
    {
        try
        {
            string query = "SELECT COUNT(*) FROM Staff WHERE LOWER(Email) = LOWER(@E)";

            // Er wordt een connectie met de DataBase gemaakt met de bovenstaande query
            using (var connection = _connector.CreateDbConnection())
            using (var command = new MySqlCommand(query, (MySqlConnection)connection))
            {
                // De paramater wordt ingevuld
                command.Parameters.AddWithValue("@E", email);
                bool inUse = Convert.ToInt32(await command.ExecuteScalarAsync()) > 0;

                // Als de uitvoer van de query > 0, dan inUse = true, "Email detected", anders inUse = false, "No email detected"
                return (inUse, inUse ? "Email detected" : "No email detected");
            }
        }

        catch (MySqlException ex)
        {
            await Console.Error.WriteLineAsync($"Database error: {ex.Message} Email");
            return (false, ex.Message);
        }
        catch (Exception ex)
        {
            await Console.Error.WriteLineAsync($"Unexpected error: {ex.Message}");
            return (false, ex.Message);
        }
    }


    private async Task<(bool status, Dictionary<string, object> data)> GetUserDataAsync(string userid)
    {
        try
        {
            string query = "SELECT FirstName, LastName, Adres, Email, TelNum FROM UserCustomer WHERE ID = @I";

            // Er wordt een connectie met de DataBase gemaakt met de bovenstaande query
            using (var connection = _connector.CreateDbConnection())
            using (var command = new MySqlCommand(query, (MySqlConnection)connection))
            {
                // De parameter wordt ingevuld
                command.Parameters.AddWithValue("@I", userid);

                using (var reader = await command.ExecuteReaderAsync())
                {
                    // Alle gegevens in de row worden verzameld <Kolom naam : Kolom data>
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
            await Console.Error.WriteLineAsync($"Database error: {ex.Message} User");
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

            // Er wordt een connectie met de DataBase gemaakt met de bovenstaande query
            using (var connection = _connector.CreateDbConnection())
            using (var command = new MySqlCommand(query, (MySqlConnection)connection))
            {
                // De parameter wordt ingevuld
                command.Parameters.AddWithValue("@I", carId);

                using (var reader = await command.ExecuteReaderAsync())
                {
                    // Alle gegevens in de row worden verzameld <Kolom naam : Kolom data>
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
            await Console.Error.WriteLineAsync($"Database error: {ex.Message} Vehicle");
            return (false, null);
        }
        catch (Exception ex)
        {
            await Console.Error.WriteLineAsync($"Unexpected error: {ex.Message}");
            return (false, null);
        }
    }

    public async Task<(bool status, List<string> ids)> GetReviewIdsAsync(string user, string userId)
    {
        try
        {
            bool isVehicleManager = user.Equals("vehicleManager");
            string query = "QUERY";

            if (isVehicleManager)
            {
                query = "SELECT OrderId FROM Abonnement WHERE Status = 'requested' AND VMStatus = 'requested' AND KvK = @K";

            }
            else if (user.Equals("frontOffice"))
            {
                query = "SELECT OrderId FROM Abonnement WHERE Status = 'requested' AND (VMStatus = 'X' OR VMStatus = 'accepted')";
            }

            // Er wordt een connectie met de DataBase gemaakt met de bovenstaande query
            using (var connection = _connector.CreateDbConnection())
            using (var command = new MySqlCommand(query, (MySqlConnection)connection))
            
            {
                if (isVehicleManager)
                {
                    // De parameter wordt ingevuld
                    command.Parameters.AddWithValue("@K", GetKvK(userId));
                }

                using (var reader = await command.ExecuteReaderAsync())
                {
                    List<string> ids = new List<string>();
                    while (await reader.ReadAsync())
                    {
                        // Alle ids worden in een list gestopt
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            ids.Add(reader.GetValue(i).ToString());
                        }
                    }

                    return (true, ids);
                }
            }
        }
        catch (MySqlException ex)
        {
            await Console.Error.WriteLineAsync(ex.Message);
            return (false, null);
        }
        catch (Exception ex)
        {
            await Console.Error.WriteLineAsync(ex.Message);
            return (false, null);
        }
    }

    public async Task<(bool status, List<Dictionary<string, object>> data)> GetReviewAsync(string id)
    {
        try
        {
            string query = "SELECT * FROM Abonnement WHERE OrderId = @I";

            // Er wordt een connectie met de DataBase gemaakt met de bovenstaande query
            using (var connection = _connector.CreateDbConnection())
            using (var command = new MySqlCommand(query, (MySqlConnection)connection))
            {
                // De parameter wordt ingevuld
                command.Parameters.AddWithValue("@I", id);

                // De query wordt uitgevoerd
                using (var reader = await command.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        // Er wordt een lijst gemaakt met alle waarders van elke row van de uitgevoerde query
                        var rows = new List<Dictionary<string, object>>();
                        // De gegevens in de row worden opgeslagen in een dictonary <Kolom naam : Kolom data>
                        var row = new Dictionary<string, object>();

                        var getUserData = GetUserDataAsync(reader.GetValue(5).ToString());
                        var getVehiceData = GetVehicleData(reader.GetValue(4).ToString());

                        // alle gegvens in de row worden verzameld
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            row = new Dictionary<string, object>();
                            row[reader.GetName(i)] = reader.GetValue(i);
                            rows.Add(row);
                        }

                        var userData = await getUserData;

                        // Alle gebruikers gegevens worden verzameld
                        foreach (var userField in userData.data)
                        {
                            row = new Dictionary<string, object>();
                            row[userField.Key] = userField.Value;
                            rows.Add(row);
                        }

                        var vehicleData = await getVehiceData;

                        // Alle voertuig gegevens worden verzameld
                        foreach (var vehicelField in vehicleData.data)
                        {
                            row = new Dictionary<string, object>();
                            row[vehicelField.Key] = vehicelField.Value;
                            rows.Add(row);
                        }

                        return (true, rows);
                    }

                    return (false, null);
                }
            }
        }
        catch (MySqlException ex)
        {
            await Console.Error.WriteLineAsync($"Database error: {ex.Message} Review");
            return (false, null);
        }
        catch (Exception ex)
        {
            await Console.Error.WriteLineAsync($"Unexpected error: {ex.Message}");
            return (false, null);
        }
    }

    private string GetKvK(string id)
    {
        try
        {
            string query = "SELECT Business FROM VehicleManager WHERE ID = @I";

            // Er wordt een connectie met de DataBase gemaakt met de bovenstaande query
            using (var connection = _connector.CreateDbConnection())
            using (var command = new MySqlCommand(query, (MySqlConnection)connection))
            {
                // De parameter wordt ingevuld
                command.Parameters.AddWithValue("@I", id);

                return command.ExecuteScalar().ToString();
            }
        }
        catch (MySqlException ex)
        {
            Console.WriteLine(ex.Message);
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return null;
        }
    }

    public async Task<(bool status, string message)> SetStatusAsync(string id, string status, string employee, string userType)
    {
        try
        {
            bool isOfficeType = userType.Equals("frontOffice");
            string query = "QUERY";

            if (isOfficeType)
            {
                query = "UPDATE Abonnement SET Status = @S, ReviewedBy = @E WHERE OrderId = @I";
            }
            else if (userType.Equals("vehicleManager"))
            {
                query = "UPDATE Abonnement SET VMStatus = @S WHERE OrderId = @I";
            }

            // Er wordt een connectie met de DataBase gemaakt met de bovenstaande query
            using (var connection = _connector.CreateDbConnection())
            using (var command = new MySqlCommand(query, (MySqlConnection)connection))
            {
                // De parameters worden ingevuld
                command.Parameters.AddWithValue("@S", status);
                command.Parameters.AddWithValue("@I", id);

                if (isOfficeType)
                {
                    command.Parameters.AddWithValue("@E", employee);
                }

                if (await command.ExecuteNonQueryAsync() > 0)
                {
                    // Er wordt gekeken of de gegevens zijn ingevoerd in de DataBase
                    return (true, "Status updated");
                }
                
                return (false, "Status not updated");
            }
        }
        catch (MySqlException ex)
        {
            return (false, ex.Message);
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }
}