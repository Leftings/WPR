using System.Data;
using Microsoft.VisualBasic;
using MySql.Data.MySqlClient;
using Org.BouncyCastle.Asn1.Mozilla;
using WPR.Controllers.Rental;
using WPR.Cryption;
using WPR.Database;
using WPR.Services;

namespace WPR.Repository;

/// <summary>
/// De VehicleRepository class geeft de methodes voor de interactie met de database voor gebruiker gerelateerde operaties.
/// Het implementeert de IVehicleRepository interface.
/// </summary>
public class VehicleRepository : IVehicleRepository
{
    private readonly Connector _connector;
    private readonly Crypt _crypt;
    private readonly EmailService _emailService;

    public VehicleRepository(Connector connector, Crypt crypt, EmailService emailService)
    {
        _connector = connector ?? throw new ArgumentNullException(nameof(connector));
        _crypt = crypt ?? throw new ArgumentNullException(nameof(crypt));
        _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
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

    /// <summary>
    /// Alle framenummers van de voertuigen worden verzameld en in een list gestopt
    /// </summary>
    /// <returns></returns>
    public async Task<List<string>> GetFrameNumbersAsync()
    {
        try
        {
            string query = "SELECT FrameNr FROM Vehicle";

            // Er wordt een connectie aangemaakt met de DataBase met bovenstaande query 
            using (var connection = _connector.CreateDbConnection())
            using (var command = new MySqlCommand(query, (MySqlConnection)connection))
            using (var reader = await command.ExecuteReaderAsync())
            {
                // Er wordt een lijst met alle frameNrs
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

    /// <summary>
    /// Alle framenummers van een specifiek voertuigtype worden verzameld en in een list gestopt
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public async Task<List<string>> GetFrameNumberSpecifiekTypeAsync(string type)
    {
        try
        {
            string query = "SELECT FrameNr FROM Vehicle WHERE Sort = @S";

            // Er wordt een connectie aangemaakt met de DataBase met bovenstaande query 
            using (var connection = _connector.CreateDbConnection())
            using (var command = new MySqlCommand(query, (MySqlConnection)connection))
            {
                // De parameter wordt ingevuld
                command.Parameters.AddWithValue("@S", type);

                //Er wordt een lijst gemaakt met alle FrameNrs van een specifieke voertuigsoort
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

    /// <summary>
    /// Er wordt doormiddel van een specifiek framenummer alle gegevens van dit voertuig verzameld.
    /// De gegevens worden in een list gestopt die bestaat uit dictonaries.
    /// In de dictonaries wordt als key de colomnaam gebruikt en als waarde de data van de colom
    /// </summary>
    /// <param name="frameNr"></param>
    /// <returns></returns>
    public async Task<List<Dictionary<object, string>>> GetVehicleDataAsync(string frameNr)
    {
        try
        {
            string query = "SELECT * FROM Vehicle WHERE FrameNR = @F";

            // Er wordt een connectie aangemaakt met de DataBase met bovenstaande query 
            using (var connection = _connector.CreateDbConnection())
            using (var command = new MySqlCommand(query, (MySqlConnection)connection))
            {
                // De parameter wordt ingevuld
                command.Parameters.AddWithValue("@F", frameNr);

                // Er wordt een lijst aangemaakt met alle gegevens van het voertuig
                var data = new List<Dictionary<object, string>>();
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            // Van elke row worden colom namen met gegevens vastgesteld 
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

    private async Task<(bool Status, object Message)> DecryptCookie(string userId)
    {
        try
        {
            return(true, Convert.ToInt32(_crypt.Decrypt(userId)));
        }
        catch (OverflowException ex)
        {
            return (false, ex.Message);
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }

    private async Task<(bool Status, string Message)> InsertRequest(RentalRequest request, object userId)
    {
        try
        {
            string query = "INSERT INTO Abonnement (StartDate, EndDate, Price, FrameNrCar, Customer) VALUES (@S, @E, @P, @F, @C)";
            using (var connection = _connector.CreateDbConnection())
            using (var command = new MySqlCommand(query, (MySqlConnection)connection))
            {
                command.Parameters.AddWithValue("@S", request.StartDate);
                command.Parameters.AddWithValue("@E", request.EndDate);
                command.Parameters.AddWithValue("@P", request.Price);
                command.Parameters.AddWithValue("@F", request.FrameNrCar);
                command.Parameters.AddWithValue("@C", userId);

                if (command.ExecuteNonQuery() > 0)
                {
                    return (true, "Inserted");
                }
                return (false, "Data Not Inserted");
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

    private async Task<(bool Status, string Message)> SendEmail(RentalRequest request)
    {
        try
        {
            int frameNr = Convert.ToInt32(request.FrameNrCar);
            var vehicleName = GetVehicleNameAsync(frameNr);
            var vehiclePlate = GetVehiclePlateAsync(frameNr);
            var vehicleColor = GetVehicleColorAsync(frameNr);

            await _emailService.SendRentalConfirmMail(
                toEmail: request.Email,
                carName: await vehicleName,
                carColor: await vehicleColor,
                carPlate: await vehiclePlate,
                startDate: request.StartDate,
                endDate: request.EndDate,
                price: request.Price.ToString()
            );

            return (true, "Huur succesvol aangemaakt.");
            
        }
        catch (OverflowException ex)
        {
            return (false, ex.Message);
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }

    public async Task<(bool Status, string Message)> HireVehicle(RentalRequest request, string userId)
    {
        var idUser = await DecryptCookie(userId);

        if (!idUser.Status)
        {
            return (false, (string)idUser.Message);
        }

        var insertData = InsertRequest(request, idUser.Message);
        var emailService = SendEmail(request);

        (bool Status, string Message) insertDataReponse = await insertData;
        if (!insertDataReponse.Status)
        {
            return (false, insertDataReponse.Message);
        }

        (bool Status, string Message) emailServiceResponse = await emailService;
        if (!emailServiceResponse.Status)
        {
            return (false, emailServiceResponse.Message);
        }

        return (true, emailServiceResponse.Message);
    }
}