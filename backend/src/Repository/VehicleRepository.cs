using System.Data;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualBasic;
using MySql.Data.MySqlClient;
using Org.BouncyCastle.Asn1.Mozilla;
using Org.BouncyCastle.Bcpg;
using WPR.Controllers.Customer.Rental;
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
            string query = "SELECT FrameNr FROM Vehicle ORDER BY FIELD(Sort, 'Car', 'Camper', 'Caravan'), FrameNr DESC";

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

    private async Task<(bool Status, int StatusCode, object Message)> DecryptCookie(string userId)
    {
        try
        {
            return(true, 200, Convert.ToInt32(_crypt.Decrypt(userId)));
        }
        catch (OverflowException ex)
        {
            return (false, 500, ex.Message);
        }
        catch (Exception ex)
        {
            return (false, 500, ex.Message);
        }
    }

    private async Task<bool> IsBusinessUser(object userId)
    {
        try
        {
            string queryBusiness = "SELECT AccountType FROM Customer WHERE ID = @I";
            using (var connection = _connector.CreateDbConnection())
            using (var commandBusiness = new MySqlCommand(queryBusiness, (MySqlConnection)connection))
            {
                commandBusiness.Parameters.AddWithValue("@I", userId);
                using (var reader = await commandBusiness.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync() && ((string)reader.GetValue(0)).Equals("Business"))
                    {
                        return true;
                    }
                    return false;
                }
            }
        }
        catch (MySqlException ex)
        {
            Console.WriteLine(ex.Message);
            throw;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            throw;
        }
    }

    private async Task<(bool Status, string Message)> InsertRequest(RentalRequest request, object userId)
    {
        try
        {
            var isBusinessUser = IsBusinessUser(userId);

            using (var connection = _connector.CreateDbConnection())
            {
                bool resultIsBusinessUser = await isBusinessUser;

                string query = resultIsBusinessUser ? 
                "INSERT INTO Contract (StartDate, EndDate, Price, FrameNrVehicle, Customer, VMStatus) VALUES (@S, @E, @P, @F, @C, @V)"
                :
                "INSERT INTO Contract (StartDate, EndDate, Price, FrameNrVehicle, Customer) VALUES (@S, @E, @P, @F, @C)";
                using (var command = new MySqlCommand(query, (MySqlConnection)connection))
                {
                    command.Parameters.AddWithValue("@S", request.StartDate);
                    command.Parameters.AddWithValue("@E", request.EndDate);
                    command.Parameters.AddWithValue("@P", request.Price);
                    command.Parameters.AddWithValue("@F", request.FrameNrVehicle);
                    command.Parameters.AddWithValue("@C", userId);

                    if (resultIsBusinessUser)
                    {
                        command.Parameters.AddWithValue("@V", "requested");
                    }

                    if (await command.ExecuteNonQueryAsync() > 0)
                    {
                        return (true, "Inserted");
                    }
                    return (false, "Data Not Inserted");
                }
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
            int frameNr = Convert.ToInt32(request.FrameNrVehicle);
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

    public async Task<(bool Status, int StatusCode, string Message)> CancelRental(int rentalId, string userCookie)
    {
        try
        {
            var userId = DecryptCookie(userCookie);
            string query = "DELETE FROM Contract WHERE OrderId = @Id AND Customer = @Customer";

            using (var connection = _connector.CreateDbConnection())
            using (var contractCommand = new MySqlCommand(query, (MySqlConnection)connection))
            {
                (bool Status, int StatusCode, object Message) userIdResponse = await userId;

                if (userIdResponse.Status)
                {
                    contractCommand.Parameters.AddWithValue("@Id", rentalId);
                    contractCommand.Parameters.AddWithValue("@Customer", Convert.ToInt32(userIdResponse.Message));
                    
                    if (contractCommand.ExecuteNonQuery() > 0)
                    {
                        return (true, 200, "Rental cancelled successfully");
                    }
                    else
                    {
                        return (false, 405, "Rental not found or you do not have permission to cancel this rental");
                    }
                }
                return (false, userIdResponse.StatusCode, (string)userIdResponse.Message);
            }
        }
        catch (OverflowException ex)
        {
            return (false, 500, ex.Message);
        }
        catch (Exception ex)
        {
            return (false, 500, ex.Message);
        }
    }

    public async Task<(bool Status, int StatusCode, string Message, IList<object> UserRentals)> GetAllUserRentals(string userCookie)
    {
        try
        {
            var userId = DecryptCookie(userCookie);
            string query = "SELECT OrderId, FrameNrVehicle, StartDate, EndDate, Price, Status FROM Contract WHERE Customer = @Customer";

            // (Dictionary geen betere oplossing?)
            var rentals = new List<object>();

            using (var connection = _connector.CreateDbConnection())
            using (var command = new MySqlCommand(query, (MySqlConnection)connection))
            {
                (bool Status, int StatusCode, object Message) userIdResponse = await userId;

                if (userIdResponse.Status)
                {
                    command.Parameters.AddWithValue("@Customer", Convert.ToInt32(userIdResponse.Message));

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (reader.Read())
                        {
                            string carName = await GetVehicleNameAsync(reader.GetInt32(1));
                            string licensePlate = await GetVehiclePlateAsync(reader.GetInt32(1));

                            rentals.Add(new
                            {
                                Id = reader.IsDBNull(0) ? (int?)null : reader.GetInt32(0),
                                FrameNrVehicle = reader.IsDBNull(1) ? (int?)null : reader.GetInt32(1),
                                CarName = carName,
                                LicensePlate = licensePlate,
                                StartDate = reader.IsDBNull(2) ? (DateTime?)null : reader.GetDateTime(2),
                                EndDate = reader.IsDBNull(3) ? (DateTime?)null : reader.GetDateTime(3),
                                Price = reader.IsDBNull(4) ? (decimal?)null : reader.GetDecimal(4),
                                Status = reader.IsDBNull(5) ? null : reader.GetString(5),
                            });
                        }
                    }
                    return (true, 200, "Data Collected", rentals);
                }
                return (false, userIdResponse.StatusCode, (string)userIdResponse.Message, new List<object>());
            }
        }
        catch (MySqlException ex)
        {
            return (false, 500, ex.Message, new List<object>());
        }
        catch (OverflowException ex)
        {
            return (false, 500, ex.Message, new List<object>());
        }
        catch (Exception ex)
        {
            return (false, 500, ex.Message, new List<object>());
        }
    }

    public (bool Status, int StatusCode, string Message, IList<object> UserRentals) GetAllUserRentalsDetailed()
    {
        try
            {
            string query = "SELECT OrderId, StartDate, EndDate, Price, FrameNrVehicle, Customer, Status, ReviewedBy, VMStatus FROM Contract";

            var rentals = new List<object>();

            using (var connection = _connector.CreateDbConnection())
            using (var command = new MySqlCommand(query, (MySqlConnection)connection))
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    rentals.Add(new
                    {
                        ID = reader.IsDBNull(0) ? (int?)null : reader.GetInt32(0),
                        StartDate = reader.IsDBNull(1) ? (DateTime?)null : reader.GetDateTime(1),
                        EndDate = reader.IsDBNull(2) ? (DateTime?)null : reader.GetDateTime(2),
                        Price = reader.IsDBNull(3) ? (decimal?)null : reader.GetDecimal(3),
                        FrameNrVehicle = reader.IsDBNull(4) ? (int?)null : reader.GetInt32(4),
                        Customer = reader.IsDBNull(5) ? (int?)null : reader.GetInt32(5),
                        Status = reader.IsDBNull(6) ? null : reader.GetString(6),
                        ReviewedBy = reader.IsDBNull(7) ? null : reader.GetString(7),
                        VMStatus = reader.IsDBNull(8) ? null : reader.GetString(8),
                    });
                }
                return (true, 200, "Succesfully Collected Data", rentals);
            }
        }
        catch (MySqlException ex)
        {
            return (false, 500, ex.Message, new List<object>());
        }
        catch (Exception ex)
        {
            return (false, 500, ex.Message, new List<object>());
        }
    }

    public (bool Status, int StatusCode, string Message) ChangeRental(UpdateRentalRequest request)
    {
        try
        {
            string query = "UPDATE Contract SET StartDate = @StartDate, EndDate = @EndDate, Price = @Price WHERE OrderId = @Id";
            using (var connection = _connector.CreateDbConnection())
            using (var command = new MySqlCommand(query, (MySqlConnection)connection))
            {
                command.Parameters.AddWithValue("@StartDate", request.StartDate);
                command.Parameters.AddWithValue("@EndDate", request.EndDate);
                command.Parameters.AddWithValue("@Price", request.Price);
                command.Parameters.AddWithValue("@Id", request.Id);

                if (command.ExecuteNonQuery() > 0)
                {
                    return (true, 200, "Rental Updated Successfully");
                }

                return (false, 501,  "Rental wasn't updated");
            }
        }
        catch (MySqlException ex)
        {
            return (false, 500, ex.Message);
        }
        catch (Exception ex)
        {
            return (false, 500, ex.Message);
        }
    }

    public async Task<(bool Status, string Message)> DeleteVehicleAsync(string frameNr)
    {
        try
        {
            string queryCustomer = "DELETE FROM Vehicle WHERE frameNr = @FrameNr";

            using (var connection = _connector.CreateDbConnection())
            using (var customerCommand = new MySqlCommand(queryCustomer, (MySqlConnection)connection))
            {
                customerCommand.Parameters.AddWithValue("@FrameNr", frameNr);
                
                int rowsAffected = await customerCommand.ExecuteNonQueryAsync();

                if (rowsAffected > 0)
                {
                    return (true, "Vehicle deleted");
                }
                else
                {
                    return (false, "Vehicle could not be deleted");
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
}