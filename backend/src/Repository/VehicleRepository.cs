using System.Data;
using MySql.Data.MySqlClient;
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
}