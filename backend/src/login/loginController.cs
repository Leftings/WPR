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

    public LoginController(Connector connector)
    {
        _connector = connector ?? throw new ArgumentNullException(nameof(connector));
    }

    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginRequest loginRequest)
    {
        if (loginRequest == null || string.IsNullOrEmpty(loginRequest.Email) || string.IsNullOrEmpty(loginRequest.Password))
        {
            return BadRequest(new { message = "Invalid input. Please provide email and password." });
        }

        try
        {
            using (var connection = _connector.CreateDbConnection())
            {
                string table = loginRequest.IsEmployee ? "Staff" : "User_Customer";

                string query = $@"SELECT 1 FROM {table} WHERE LOWER(email) = LOWER(@Email) AND BINARY password = @Password";

                using (var command = new MySqlCommand(query, (MySqlConnection)connection))
                {
                    command.Parameters.AddWithValue("@Email", loginRequest.Email);
                    command.Parameters.AddWithValue("@Password", loginRequest.Password);

                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            return Ok(new { message = "Login successful" });
                        }
                        else
                        {
                            Console.WriteLine(Unauthorized(new { message = "Invalid credentials" }));
                            return Unauthorized(new { message = "Invalid credentials" });
                        }
                    }
                }
            }
        }
        catch (MySqlException ex)
        {
            Console.Error.WriteLine($"Database error: {ex.Message}");
            return StatusCode(500, new { message = "An error occurred while accesing the database." });
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Unexpected error: {ex.Message}");
            return StatusCode(500, new { message = "An error occurred while processing your request." });
        }
    }
}

public class LoginRequest
{
    public string Email { get; set; }
    public string Password { get; set; }
    public bool IsEmployee { get; set; }
}

