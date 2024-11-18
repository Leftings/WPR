using WPR.Repository;

namespace WPR.Login;

using Microsoft.AspNetCore.Mvc;
using WPR.Database;
using WPR.Data;
using WPR.Repository;
using MySql.Data.MySqlClient;
using System;

[Route("api/[controller]")]
[ApiController]
public class LoginController : ControllerBase
{
    private readonly IUserRepository _userRepository;

    public LoginController(IUserRepository userRepository)
    {
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
    }

    [HttpPost("login")]
    public async Task <IActionResult> Login([FromBody] LoginRequest loginRequest)
    {
        if (loginRequest == null || string.IsNullOrEmpty(loginRequest.Email) || string.IsNullOrEmpty(loginRequest.Password))
        {
            return BadRequest(new { message = "Invalid input. Please provide email and password." });
        }

        try
        {
            string table = loginRequest.IsEmployee ? "Staff" : "User_Customer";
            if (table != "Staff" && table != "User_Customer")
            {
                return BadRequest(new { message = "Invalid input. Please provide employee or user name." });
            }
            
            bool isValid = await _userRepository.ValidateUserAsync(loginRequest.Email, loginRequest.Password, loginRequest.IsEmployee);

            if (isValid)
            {
                return Ok(new { message = "Login Successful." });
            }
            else
            {
                return Unauthorized(new { message = "Login Failed." });
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

