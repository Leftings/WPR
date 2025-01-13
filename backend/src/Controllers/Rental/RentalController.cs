using Microsoft.AspNetCore.Mvc;
using System;
using System.Data;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using WPR.Controllers.Rental;
using WPR.Database;
using WPR.Cryption;
using WPR.Repository;
using WPR.Services;

namespace WPR.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RentalController : ControllerBase
    {
        private readonly Connector _connector;
        private readonly Crypt _crypt;
        private readonly EmailService _emailService;
        private readonly VehicleRepository _vehicleRepo;

        public RentalController(Connector connector, Crypt crypt, EmailService emailService,
            VehicleRepository vehicleRepo)
        {
            _connector = connector ?? throw new ArgumentNullException(nameof(connector));
            _crypt = crypt ?? throw new ArgumentNullException(nameof(crypt));
            _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
            _vehicleRepo = vehicleRepo ?? throw new ArgumentNullException(nameof(vehicleRepo));
        }

        [HttpPost("CreateRental")]
        public async Task<IActionResult> CreateRental([FromBody] RentalRequest rentalRequest)
        {
            // Log de binnenkomende aanvraag
            Console.WriteLine("CreateRental called with request: " +
                              $"FrameNrCar: {rentalRequest.FrameNrCar}, " +
                              $"StartDate: {rentalRequest.StartDate}, " +
                              $"EndDate: {rentalRequest.EndDate}, " +
                              $"Price: {rentalRequest.Price}");

            // Valideer de invoer
            if (rentalRequest == null)
            {
                return BadRequest(new { message = "Huurverzoek kan niet null zijn." });
            }

            if (string.IsNullOrWhiteSpace(rentalRequest.FrameNrCar) ||
                rentalRequest.StartDate == default ||
                rentalRequest.EndDate == default ||
                rentalRequest.Price <= 0)
            {
                return BadRequest(new { message = "Ongeldige invoergegevens." });
            }

            if (rentalRequest.StartDate >= rentalRequest.EndDate)
            {
                return BadRequest(new { message = "Startdatum moet voor einddatum zijn." });
            }

            // Haal CustomerID uit de cookie
            string loginCookie = HttpContext.Request.Cookies["LoginSession"];
            if (string.IsNullOrEmpty(loginCookie))
            {
                Console.WriteLine("Geen login sessie cookie gevonden");
                return Unauthorized(new { message = "Geen actieve sessie." });
            }

            int userId;
            try
            {
                userId = int.Parse(_crypt.Decrypt(loginCookie));
                Console.WriteLine($"Gedecodeerde Gebruiker ID: {userId}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Fout bij het decoderen van cookie: {ex.Message}");
                return Unauthorized(new { message = "Ongeldige sessie." });
            }

            try
            {
                using (var connection = _connector.CreateDbConnection())
                {
                    if (connection.State != ConnectionState.Open)
                    {
                        connection.Open();
                        Console.WriteLine("Databaseverbinding geopend.");
                    }

                    using (var transaction = connection.BeginTransaction())
                    {
                        try
                        {
                            string insertAbbonementQuery = @"
INSERT INTO Abonnement (StartDate, EndDate, Price, FrameNrCar, Customer, Status, ReviewedBy) 
VALUES (@StartDate, @EndDate, @Price, @FrameNrCar, @Customer, @Status, @ReviewedBy)";
                            using (var command = connection.CreateCommand())
                            {
                                command.CommandText = insertAbbonementQuery;
                                command.CommandType = CommandType.Text;

                                var startDateParam = command.CreateParameter();
                                startDateParam.ParameterName = "@StartDate";
                                startDateParam.Value = rentalRequest.StartDate;

                                var endDateParam = command.CreateParameter();
                                endDateParam.ParameterName = "@EndDate";
                                endDateParam.Value = rentalRequest.EndDate;

                                var priceParam = command.CreateParameter();
                                priceParam.ParameterName = "@Price";
                                priceParam.Value = Convert.ToDecimal(rentalRequest.Price);

                                var frameNrParam = command.CreateParameter();
                                frameNrParam.ParameterName = "@FrameNrCar";
                                frameNrParam.Value = rentalRequest.FrameNrCar;

                                var customerParam = command.CreateParameter();
                                customerParam.ParameterName = "@Customer";
                                customerParam.Value = userId;

                                var statusParam = command.CreateParameter();
                                statusParam.ParameterName = "@Status";
                                statusParam.Value = "requested";

                                var reviewedByParam = command.CreateParameter();
                                reviewedByParam.ParameterName = "@ReviewedBy";

                                // Als de status 'requested' is, stellen we ReviewedBy in op NULL
                                if (statusParam.Value.ToString() == "requested")
                                {
                                    reviewedByParam.Value = DBNull.Value;
                                }
                                else
                                {
                                    reviewedByParam.Value = DBNull.Value;
                                }

                                command.Parameters.Add(startDateParam);
                                command.Parameters.Add(endDateParam);
                                command.Parameters.Add(priceParam);
                                command.Parameters.Add(frameNrParam);
                                command.Parameters.Add(customerParam);
                                command.Parameters.Add(statusParam);
                                command.Parameters.Add(reviewedByParam);

                                command.ExecuteNonQuery();
                            }

                            transaction.Commit();
                            Console.WriteLine("Transactie succesvol gecommit.");

                            string carName =
                                await _vehicleRepo.GetVehicleNameAsync(int.Parse(rentalRequest.FrameNrCar));
                            if (string.IsNullOrEmpty(carName))
                            {
                                return NotFound(new { message = "Car name not found for the frameNr" });
                            }

                            string carPlate =
                                await _vehicleRepo.GetVehiclePlateAsync(int.Parse(rentalRequest.FrameNrCar));
                            if (string.IsNullOrEmpty(carPlate))
                            {
                                return NotFound(new { message = "License plate not found for frameNr" });
                            }

                            string carColor =
                                await _vehicleRepo.GetVehicleColorAsync(int.Parse(rentalRequest.FrameNrCar));
                            if (string.IsNullOrEmpty(carColor))
                            {
                                return NotFound(new { message = "Color not found for frameNr" });
                            }

                            await _emailService.SendRentalConfirmMail(
                                toEmail: rentalRequest.Email,
                                carName: carName,
                                carColor: carColor,
                                carPlate: carPlate,
                                startDate: rentalRequest.StartDate,
                                endDate: rentalRequest.EndDate,
                                price: rentalRequest.Price.ToString()
                            );

                            return Ok(new { message = "Huur succesvol aangemaakt." });
                        }
                        catch (Exception transEx)
                        {
                            transaction.Rollback();
                            Console.WriteLine($"Transactie teruggedraaid: {transEx.Message}");
                            throw;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Fout in CreateRental: {ex.Message}\n{ex.StackTrace}");
                return StatusCode(500,
                    new { message = $"Er is een fout opgetreden bij het aanmaken van de huur: {ex.Message}" });
            }
        }

        [HttpDelete("CancelRental")]
        public async Task<IActionResult> CancelRentalAsync(int rentalId, int frameNr)
        {
            string loginCookie = HttpContext.Request.Cookies["LoginSession"];
            int userId = Convert.ToInt32(_crypt.Decrypt(loginCookie));
            bool deletion1 = false;
            bool deletion2 = false;

            try
            {
                string query1 = @"
            DELETE FROM Abonnement WHERE OrderId = @Id AND Customer = @Customer";

                using (var connection = _connector.CreateDbConnection())
                {
                    using (var abonnementCommand = new MySqlCommand(query1, (MySqlConnection)connection))
                    {
                        abonnementCommand.Parameters.AddWithValue("@Id", rentalId);
                        abonnementCommand.Parameters.AddWithValue("@Customer", userId);
                        

                        int rowsAffectedAbonnement = await abonnementCommand.ExecuteNonQueryAsync();
                        if (rowsAffectedAbonnement > 0)
                        {
                            return Ok(new { message = "Rental cancelled successfully" });
                        }
                        else
                        {
                            return NotFound(new
                                { message = "Rental not found or you do not have permission to cancel this rental" });

                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return StatusCode(500, "An error occurred while cancelling rental");
            }
        }


        [HttpGet("GetAllUserRentals")]
        public async Task<IActionResult> GetAllUserRentalsAsync()
        {
            string loginCookie = HttpContext.Request.Cookies["LoginSession"];
            int userId = Convert.ToInt32(_crypt.Decrypt(loginCookie));
            ;

            try
            {
                string query = @"
            SELECT OrderId, FrameNrCar, StartDate, EndDate, Price, Status
            FROM Abonnement 
            WHERE Customer = @Customer";

                var rentals = new List<object>();

                using (var connection = _connector.CreateDbConnection())
                {
                    using (var command = new MySqlCommand(query, (MySqlConnection)connection))
                    {
                        // Ensure userId is added to the query correctly
                        command.Parameters.AddWithValue("@Customer", userId);

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (reader.Read())
                            {

                                string carName = await _vehicleRepo.GetVehicleNameAsync(reader.GetInt32(1));
                                string licensePlate = await _vehicleRepo.GetVehiclePlateAsync(reader.GetInt32(1));

                                rentals.Add(new
                                {
                                    Id = reader.IsDBNull(0) ? (int?)null : reader.GetInt32(0),
                                    FrameNrCar = reader.IsDBNull(1) ? (int?)null : reader.GetInt32(1),
                                    CarName = carName,
                                    LicensePlate = licensePlate,
                                    StartDate = reader.IsDBNull(2) ? (DateTime?)null : reader.GetDateTime(2),
                                    EndDate = reader.IsDBNull(3) ? (DateTime?)null : reader.GetDateTime(3),
                                    Price = reader.IsDBNull(4) ? (decimal?)null : reader.GetDecimal(4),
                                    Status = reader.IsDBNull(5) ? null : reader.GetString(5),
                                });
                            }
                        }
                    }
                }

                return Ok(rentals);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return StatusCode(500, "An error occurred while fetching rentals.");
            }
        }

        [HttpGet("GetAllUserRentalsWithDetails")]
        public async Task<IActionResult> GetAllUserRentalsWithDetailsAsync()
        {
            try
            {
                string query = @"
            SELECT 
                OrderId, 
                StartDate, 
                EndDate, 
                Price, 
                FrameNrCar, 
                Customer, 
                Status, 
                ReviewedBy, 
                VMStatus, 
                Kvk
            FROM Abonnement";

                var rentals = new List<object>();

                using (var connection = _connector.CreateDbConnection())
                {
                    if (connection.State != ConnectionState.Open)
                    {
                        connection.Open();
                    }

                    using (var command = new MySqlCommand(query, (MySqlConnection)connection))
                    {
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (reader.Read())
                            {
                                rentals.Add(new
                                {
                                    ID = reader.IsDBNull(0) ? (int?)null : reader.GetInt32(0),
                                    StartDate = reader.IsDBNull(1) ? (DateTime?)null : reader.GetDateTime(1),
                                    EndDate = reader.IsDBNull(2) ? (DateTime?)null : reader.GetDateTime(2),
                                    Price = reader.IsDBNull(3) ? (decimal?)null : reader.GetDecimal(3),
                                    FrameNrCar = reader.IsDBNull(4) ? (int?)null : reader.GetInt32(4),
                                    Customer = reader.IsDBNull(5) ? (int?)null : reader.GetInt32(5),
                                    Status = reader.IsDBNull(6) ? null : reader.GetString(6),
                                    ReviewedBy = reader.IsDBNull(7) ? (int?)null : reader.GetInt32(7),
                                    VMStatus = reader.IsDBNull(8) ? null : reader.GetString(8),
                                    Kvk = reader.IsDBNull(9) ? (int?)null : reader.GetInt32(9),
                                });
                            }
                        }
                    }
                }

                return Ok(rentals);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching rentals: {ex.Message}");
                return StatusCode(500, "An error occurred while fetching rentals.");
            }
        }
        
        [HttpPut("ChangeRental")]
        public async Task<IActionResult> ChangeRentalAsync([FromBody] UpdateRentalRequest request)
        {

            try
            {
                string query1 = @"
    UPDATE Abonnement SET StartDate = @StartDate, EndDate = @EndDate, Price = @Price WHERE OrderId = @Id";

                using (var connection = _connector.CreateDbConnection())
                {
                    using (var command = new MySqlCommand(query1, (MySqlConnection)connection))
                    {
                        command.Parameters.AddWithValue("@StartDate", request.StartDate);
                        command.Parameters.AddWithValue("@EndDate", request.EndDate);
                        command.Parameters.AddWithValue("@Price", request.Price);
                        command.Parameters.AddWithValue("@Id", request.Id);

                        Console.WriteLine($"Executing query with rentalId: {request.Id}, StartDate: {request.StartDate}, EndDate: {request.EndDate}, Price: {request.Price}");

                        if (await command.ExecuteNonQueryAsync() > 0)
                        {
                            return Ok(new { message = "Rental updated successfully" });
                        }

                        return BadRequest(new { message = "Rental wasn't updated" });
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return StatusCode(500, new { message = "An error occurred while processing the rental update" });
            }
        }
    }
}