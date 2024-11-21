namespace WPR.Login;

using Microsoft.AspNetCore.Mvc;
using WPR.Repository;
using MySql.Data.MySqlClient;
using System;
using WPR.Cookie;
using System.Data;

[Route("api/[controller]")]
[ApiController]
public class LoginController : ControllerBase
{
    private readonly IUserRepository _userRepository;
    private readonly SessionHandler _sessionHandler;

    public LoginController(IUserRepository userRepository)
    {
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
    }

    public IActionResult SetCookie(string email)
    {
        int userId = _userRepository.GetUserIdAsync(// email)
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

