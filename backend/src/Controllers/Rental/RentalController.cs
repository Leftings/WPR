using Microsoft.AspNetCore.Mvc;
using System;
using System.Data;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using WPR.Controllers.Rental;
using WPR.Database;
using WPR.Cryption;

namespace WPR.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RentalController : ControllerBase
    {
        private readonly Connector _connector;
        private readonly Crypt _crypt;

        public RentalController(Connector connector, Crypt crypt)
        {
            _connector = connector ?? throw new ArgumentNullException(nameof(connector));
            _crypt = crypt ?? throw new ArgumentNullException(nameof(crypt));
        }

        [HttpPost("CreateRental")]
        public async Task<IActionResult> CreateRental([FromBody] RentalRequest rentalRequest)
        {
            // Detailed logging for debugging
            Console.WriteLine("CreateRental called with request: " + 
                $"FrameNrCar: {rentalRequest.FrameNrCar}, " +
                $"StartDate: {rentalRequest.StartDate}, " +
                $"EndDate: {rentalRequest.EndDate}, " +
                $"Price: {rentalRequest.Price}");

            // Validate input more thoroughly
            if (rentalRequest == null)
            {
                return BadRequest(new { message = "Rental request cannot be null." });
            }

            // Get CustomerID from cookie
            string loginCookie = HttpContext.Request.Cookies["LoginSession"];
            if (string.IsNullOrEmpty(loginCookie))
            {
                Console.WriteLine("No login session cookie found");
                return Unauthorized(new { message = "No active session." });
            }

            int userId;
            try
            {
                userId = int.Parse(_crypt.Decrypt(loginCookie));
                Console.WriteLine($"Decrypted User ID: {userId}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error decrypting cookie: {ex.Message}");
                return Unauthorized(new { message = "Invalid session." });
            }

            // Comprehensive input validation
            if (string.IsNullOrWhiteSpace(rentalRequest.FrameNrCar))
            {
                return BadRequest(new { message = "Frame number is required." });
            }

            if (rentalRequest.StartDate == default)
            {
                return BadRequest(new { message = "Start date is required." });
            }

            if (rentalRequest.EndDate == default)
            {
                return BadRequest(new { message = "End date is required." });
            }

            if (rentalRequest.StartDate >= rentalRequest.EndDate)
            {
                return BadRequest(new { message = "Start date must be before end date." });
            }

            if (rentalRequest.Price <= 0)
            {
                return BadRequest(new { message = "Price must be positive." });
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
                            // Rest of the existing code remains the same...
                            // (Your previous implementation)

                            transaction.Commit();
                            Console.WriteLine("Transaction committed successfully.");
                            return Ok(new { message = "Rental created successfully.", rentalId = "some-id" });
                        }
                        catch (Exception transEx)
                        {
                            transaction.Rollback();
                            Console.WriteLine($"Transaction rolled back: {transEx.Message}");
                            return StatusCode(500, new { message = $"Database error: {transEx.Message}" });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in CreateRental: {ex.Message}\n{ex.StackTrace}");
                return StatusCode(500, new { message = $"Unexpected error: {ex.Message}" });
            }
        }
    }
}