using System.Data;
using Microsoft.VisualBasic;
using MySql.Data.MySqlClient;
using Org.BouncyCastle.Asn1.Mozilla;
using WPR.Database;

namespace WPR.Repository;

public class VehicleRepository : IVehicleRepository
{
    private readonly Connector _connector;

    public VehicleRepository(Connector connector)
    {
        _connector = connector ?? throw new ArgumentNullException(nameof(connector));
    }
    
    public async Task<string> GetVehiclePlateAsync(int frameNr)
    {
        try
        {
            string query = "SELECT LicensePlate FROM Vehicle WHERE FrameNr = @FrameNr";

            using (var connection = _connector.CreateDbConnection())
            using (var command = new MySqlCommand(query, (MySqlConnection)connection))
            {
                command.Parameters.AddWithValue("@FrameNr", frameNr);

                using (var reader = await command.ExecuteReaderAsync(CommandBehavior.SingleRow))
                {
                    if (reader.Read())
                    {
                        string plate = reader.IsDBNull(0) ? null : reader.GetString(0);
                        return $"{plate}";
                    }
                }
            }
            return String.Empty;
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error getting vehicle plate {e.Message}");
            throw;
        }
    }

    public async Task<string> GetVehicleNameAsync(int frameNr)
    {
        try
        {
            string query = "SELECT Brand, Type FROM Vehicle WHERE FrameNr = @FrameNr";

            using (var connection = _connector.CreateDbConnection())
            using (var command = new MySqlCommand(query, (MySqlConnection)connection))
            {
                command.Parameters.AddWithValue("@FrameNr", frameNr);

                using (var reader = await command.ExecuteReaderAsync(CommandBehavior.SingleRow))
                {
                    if (reader.Read())
                    {
                        string brand = reader.IsDBNull(0) ? null : reader.GetString(0);
                        string type = reader.IsDBNull(1) ? null : reader.GetString(1);
                        return $"{brand} {type}".Trim();
                    }
                }
            }
            return String.Empty;
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error getting vehicle name {e.Message}");
            throw;
        }
    }

    public async Task<string> GetVehicleColorAsync(int frameNr)
    {
        try
        {
            string query = "SELECT Color FROM Vehicle WHERE FrameNr = @FrameNr";

            using (var connection = _connector.CreateDbConnection())
            using (var command = new MySqlCommand(query, (MySqlConnection)connection))
            {
                command.Parameters.AddWithValue("@FrameNr", frameNr);

                using (var reader = await command.ExecuteReaderAsync(CommandBehavior.SingleRow))
                {
                    if (reader.Read())
                    {
                        string color = reader.IsDBNull(0) ? null : reader.GetString(0);
                        return $"{color}";
                    }
                }
            }
            return String.Empty;
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error getting vehicle color {e.Message}");
            throw;
        }
    }

    public async Task<List<string>> GetFrameNumbersAsync()
    {
        try
        {
            string query = "SELECT FrameNr FROM Vehicle";

            using (var connection = _connector.CreateDbConnection())
            using (var command = new MySqlCommand(query, (MySqlConnection)connection))
            using (var reader = await command.ExecuteReaderAsync())
            {
                var ids = new List<string>();
                while (await reader.ReadAsync())
                {
                    ids.Add(reader.GetValue(0).ToString());
                }

                return ids;
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

    public async Task<List<string>> GetFrameNumberSpecifiekTypeAsync(string type)
    {
        try
        {
            string query = "SELECT FrameNr FROM Vehicle WHERE Sort = @S";

            using (var connection = _connector.CreateDbConnection())
            using (var command = new MySqlCommand(query, (MySqlConnection)connection))
            {
                command.Parameters.AddWithValue("@S", type);
                var ids = new List<string>();

                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        ids.Add(reader.GetValue(0).ToString());
                    }
                }

                return ids;
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

    public async Task<List<Dictionary<object, string>>> GetVehicleDataAsync(string frameNr)
    {
        try
        {
            string query = "SELECT * FROM Vehicle WHERE FrameNR = @F";

            using (var connection = _connector.CreateDbConnection())
            using (var command = new MySqlCommand(query, (MySqlConnection)connection))
            {
                command.Parameters.AddWithValue("@F", frameNr);

                var data = new List<Dictionary<object, string>>();
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            var row = new Dictionary<object, string>();

                            if (reader.GetName(i).ToString().Equals("VehicleBlob"))
                            {
                                row[reader.GetName(i)] = Convert.ToBase64String((byte[])reader.GetValue(i));
                            }
                            else
                            {
                                row[reader.GetName(i)] = reader.GetValue(i).ToString();
                            }

                            data.Add(row);
                        }
                    }
                }

                return data;
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
}