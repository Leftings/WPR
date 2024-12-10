using System;
using System.Data;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using WPR.Database;

namespace WPR.Controllers.Rental
{
    [Route("api/Rental")]
    [ApiController]
    public class RentalController : ControllerBase
    {
        private readonly Connector _connector;

        public RentalController(Connector connector)
        {
            _connector = connector ?? throw new ArgumentNullException(nameof(connector));
        }

        [HttpPost("CreateRental")]
        public async Task<IActionResult> CreateRental([FromBody] CreateRentalRequest request)
        {
            if (request == null || request.FrameNrCar <= 0)
            {
                return BadRequest("Invalid request payload.");
            }

            try
            {
                using (var connection = _connector.CreateDbConnection())
                {
                    connection.Open();
                    Console.WriteLine("Database connection opened.");

                    // Start transaction
                    using (var transaction = connection.BeginTransaction())
                    {
                        try
                        {
                            // Get the customer ID based on email
                            string customerIdQuery = @"
                                SELECT ID FROM UserCustomer WHERE Email = @Email;";
                            int customerId;
                            using (var idCommand = new MySqlCommand(customerIdQuery, (MySqlConnection)connection, (MySqlTransaction)transaction))
                            {
                                idCommand.Parameters.AddWithValue("@Email", request.Email);
                                customerId = Convert.ToInt32(await idCommand.ExecuteScalarAsync());
                            }

                            if (customerId == 0)
                            {
                                return BadRequest("Customer not found.");
                            }

                            // Check if the combination of FrameNrCar and Customer exists in Vehicle_User
                            string vehicleUserCheckQuery = @"
                                SELECT COUNT(*) 
                                FROM Vehicle_User 
                                WHERE FrameNrCar = @FrameNrCar AND Customer = @Customer;";

                            using (var vehicleUserCheckCommand = new MySqlCommand(vehicleUserCheckQuery, (MySqlConnection)connection, (MySqlTransaction)transaction))
                            {
                                vehicleUserCheckCommand.Parameters.AddWithValue("@FrameNrCar", request.FrameNrCar);
                                vehicleUserCheckCommand.Parameters.AddWithValue("@Customer", customerId);

                                var vehicleUserExists = Convert.ToInt32(await vehicleUserCheckCommand.ExecuteScalarAsync()) > 0;

                                // If the combination does not exist, insert it into Vehicle_User
                                if (!vehicleUserExists)
                                {
                                    string insertVehicleUserQuery = @"
                                        INSERT INTO Vehicle_User (FrameNrCar, Customer) 
                                        VALUES (@FrameNrCar, @Customer);";

                                    using (var insertVehicleUserCommand = new MySqlCommand(insertVehicleUserQuery, (MySqlConnection)connection, (MySqlTransaction)transaction))
                                    {
                                        insertVehicleUserCommand.Parameters.AddWithValue("@FrameNrCar", request.FrameNrCar);
                                        insertVehicleUserCommand.Parameters.AddWithValue("@Customer", customerId);

                                        await insertVehicleUserCommand.ExecuteNonQueryAsync();
                                    }
                                }
                            }

                            // Create the rental record
                            string rentalQuery = @"
                                INSERT INTO Abbonement (StartDate, EndDate, Price, FrameNrCar, Customer)
                                VALUES (@StartDate, @EndDate, @Price, @FrameNrCar, @Customer);";

                            using (var rentalCommand = new MySqlCommand(rentalQuery, (MySqlConnection)connection, (MySqlTransaction)transaction))
                            {
                                rentalCommand.Parameters.AddWithValue("@StartDate", request.StartDate);
                                rentalCommand.Parameters.AddWithValue("@EndDate", request.EndDate);
                                rentalCommand.Parameters.AddWithValue("@Price", request.Price);
                                rentalCommand.Parameters.AddWithValue("@FrameNrCar", request.FrameNrCar);
                                rentalCommand.Parameters.AddWithValue("@Customer", customerId);

                                await rentalCommand.ExecuteNonQueryAsync();
                            }

                            // Commit the transaction
                            transaction.Commit();

                            return Ok("Rental created successfully.");
                        }
                        catch (Exception ex)
                        {
                            // Log exception details
                            Console.WriteLine($"Error: {ex.Message}");
                            Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                            transaction.Rollback();
                            return StatusCode(500, "An error occurred while creating the rental.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Log exception details
                Console.WriteLine($"Error: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                return StatusCode(500, "An error occurred while processing the request.");
            }
        }
    }
}
