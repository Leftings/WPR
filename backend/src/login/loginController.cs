namespace WPR.Login;

using Microsoft.AspNetCore.Mvc;
using WPR.Database;
using WPR.Data;
using MySql.Data.MySqlClient;
using System;

[Route("api/[controller]")]
[ApiController]
public class LoginController : ControllerBase
{
    private readonly Connector _connector;

    public LoginController()
    {
        _connector = new Connector(new EnvConfig());
    }

    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginRequest loginRequest)
    {
        // Handle empty or invalid requests
        if (loginRequest == null || string.IsNullOrEmpty(loginRequest.email) || string.IsNullOrEmpty(loginRequest.Password))
        {
            return BadRequest(new { message = "Invalid input. Please provide email and password." });
        }

        try
        {
            using (var connection = _connector.CreateDbConnection())
            {

                string table = loginRequest.isEmployee ? "Staff" : "User_Customer";

                string query = "SELECT * FROM " + table + " WHERE email = @email AND password = @password";

                using (var command = new MySqlCommand(query, (MySqlConnection)connection))
                {
                    command.Parameters.AddWithValue("@email", loginRequest.email);
                    command.Parameters.AddWithValue("@password", loginRequest.Password);

                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            return Ok(new { message = "Login successful" });
                        }
                        else
                        {
                            Console.WriteLine(Unauthorized(new { message = "Invalid credentials"}));
                            return Unauthorized(new { message = "Invalid credentials" });
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            // Log the error (can be replaced with actual logging mechanism like Serilog)
            Console.Error.WriteLine($"An error occurred: {ex.Message}");
            return StatusCode(500, new { message = "An error occurred while processing your request." });
        }
    }
}

public class LoginRequest
{
    public string email { get; set; }
    public string Password { get; set; }
    public bool isEmployee { get; set; }
}

