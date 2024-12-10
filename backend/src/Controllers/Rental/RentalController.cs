using Microsoft.AspNetCore.Mvc;
using System;
using System.Data;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using WPR.Controllers.Rental;
using WPR.Database;

namespace WPR.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RentalController : ControllerBase
    {
        private readonly Connector _connector;

        public RentalController(Connector connector)
        {
            _connector = connector ?? throw new ArgumentNullException(nameof(connector));
        }

        [HttpPost("CreateRental")]
        public async Task<IActionResult> CreateRental([FromBody] RentalRequest rentalRequest)
        {
            if (rentalRequest == null || string.IsNullOrWhiteSpace(rentalRequest.Email))
            {
                return BadRequest(new { message = "Email is required." });
            }

            if (string.IsNullOrWhiteSpace(rentalRequest.FrameNrCar) ||
                rentalRequest.StartDate == null ||
                rentalRequest.EndDate == null ||
                rentalRequest.Price <= 0)
            {
                return BadRequest(new { message = "Invalid input data." });
            }

            try
            {
                using (var connection = _connector.CreateDbConnection())
                {
                    if (connection.State != ConnectionState.Open)
                    {
                        connection.Open();
                        Console.WriteLine("Database connection opened.");
                    }

                    using (var transaction = connection.BeginTransaction())
                    {
                        try
                        {
                            // Retrieve the user ID by email
                            string getUserIdQuery = "SELECT ID FROM UserCustomer WHERE Email = @Email";
                            int? userId = null;

                            using (var command = connection.CreateCommand())
                            {
                                command.CommandText = getUserIdQuery;
                                command.CommandType = CommandType.Text;
                                var emailParam = command.CreateParameter();
                                emailParam.ParameterName = "@Email";
                                emailParam.Value = rentalRequest.Email;
                                command.Parameters.Add(emailParam);

                                var result = command.ExecuteScalar();
                                if (result != null)
                                {
                                    userId = Convert.ToInt32(result);
                                }
                            }

                            if (userId == null)
                            {
                                return BadRequest(new { message = "No user found with the given email." });
                            }

                            // Check if the Vehicle_User entry already exists
                            string checkVehicleUserQuery = "SELECT COUNT(*) FROM Vehicle_User WHERE FrameNrCar = @FrameNrCar AND Customer = @Customer";
                            using (var command = connection.CreateCommand())
                            {
                                command.CommandText = checkVehicleUserQuery;
                                command.CommandType = CommandType.Text;

                                var frameNrParam = command.CreateParameter();
                                frameNrParam.ParameterName = "@FrameNrCar";
                                frameNrParam.Value = rentalRequest.FrameNrCar;

                                var customerParam = command.CreateParameter();
                                customerParam.ParameterName = "@Customer";
                                customerParam.Value = userId;

                                command.Parameters.Add(frameNrParam);
                                command.Parameters.Add(customerParam);

                                var count = Convert.ToInt32(command.ExecuteScalar());
                                if (count > 0)
                                {
                                    // Update the existing Vehicle_User entry
                                    string updateVehicleUserQuery = "UPDATE Vehicle_User SET FrameNrCar = @FrameNrCar WHERE Customer = @Customer";
                                    using (var updateCommand = connection.CreateCommand())
                                    {
                                        updateCommand.CommandText = updateVehicleUserQuery;
                                        updateCommand.CommandType = CommandType.Text;

                                        var updateFrameNrParam = updateCommand.CreateParameter();
                                        updateFrameNrParam.ParameterName = "@FrameNrCar";
                                        updateFrameNrParam.Value = rentalRequest.FrameNrCar;

                                        var updateCustomerParam = updateCommand.CreateParameter();
                                        updateCustomerParam.ParameterName = "@Customer";
                                        updateCustomerParam.Value = userId;

                                        updateCommand.Parameters.Add(updateFrameNrParam);
                                        updateCommand.Parameters.Add(updateCustomerParam);

                                        updateCommand.ExecuteNonQuery();
                                    }
                                }
                                else
                                {
                                    // Insert into Vehicle_User table
                                    string insertVehicleUserQuery =
                                        "INSERT INTO Vehicle_User (FrameNrCar, Customer) VALUES (@FrameNrCar, @Customer)";
                                    using (var insertCommand = connection.CreateCommand())
                                    {
                                        insertCommand.CommandText = insertVehicleUserQuery;
                                        insertCommand.CommandType = CommandType.Text;

                                        var frameNrParamInsert = insertCommand.CreateParameter();
                                        frameNrParamInsert.ParameterName = "@FrameNrCar";
                                        frameNrParamInsert.Value = rentalRequest.FrameNrCar;

                                        var customerParamInsert = insertCommand.CreateParameter();
                                        customerParamInsert.ParameterName = "@Customer";
                                        customerParamInsert.Value = userId;

                                        insertCommand.Parameters.Add(frameNrParamInsert);
                                        insertCommand.Parameters.Add(customerParamInsert);

                                        insertCommand.ExecuteNonQuery();
                                    }
                                }
                            }

                            // Insert into Abbonement table
                            string insertAbbonementQuery = @"
                        INSERT INTO Abbonement (StartDate, EndDate, Price, FrameNrCar, Customer) 
                        VALUES (@StartDate, @EndDate, @Price, @FrameNrCar, @Customer)";
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

                                command.Parameters.Add(startDateParam);
                                command.Parameters.Add(endDateParam);
                                command.Parameters.Add(priceParam);
                                command.Parameters.Add(frameNrParam);
                                command.Parameters.Add(customerParam);

                                command.ExecuteNonQuery();
                            }

                            transaction.Commit();
                            Console.WriteLine("Transaction committed successfully.");
                            return Ok(new { message = "Rental created successfully." });
                        }
                        catch (Exception transEx)
                        {
                            transaction.Rollback();
                            Console.WriteLine($"Transaction rolled back: {transEx.Message}");
                            throw;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in CreateRental: {ex.Message}\n{ex.StackTrace}");
                return StatusCode(500, new { message = $"An error occurred while creating the rental: {ex.Message}" });
            }
        }
    }
}
