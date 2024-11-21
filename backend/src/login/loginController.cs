namespace WPR.Login;

using Microsoft.AspNetCore.Mvc;
using WPR.Repository;
using MySql.Data.MySqlClient;
using System;
using WPR.Cookie;
using System.Data;
using WPR.Database;

[Route("api/[controller]")]
[ApiController]
public class LoginController : ControllerBase
{
    private readonly IUserRepository _userRepository;
    private readonly Connector _connector;
    private readonly SessionHandler _sessionHandler;

    public LoginController(IUserRepository userRepository, Connector connector, SessionHandler sessionHandler)
    {
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _connector = connector ?? throw new ArgumentNullException(nameof(connector));
        _sessionHandler = sessionHandler ?? throw new ArgumentNullException(nameof(sessionHandler));
    }

    private async Task<IActionResult> SetCookie(LoginRequest loginRequest)
    {
        var connection = _connector.CreateDbConnection();

        try
        {
            int userId = await _userRepository.GetUserIdAsync(connection, loginRequest.Email);

            if (userId <= 0)
            {
                return BadRequest(new { message = "User ID not found." });
            }
            
            _sessionHandler.CreateCookie(Response.Cookies, "LoginSession", userId.ToString());

        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return BadRequest();
        }
        finally
        {
            connection.Close();
        }

        return Ok();
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
            bool isValid = await _userRepository.ValidateUserAsync(loginRequest.Email, loginRequest.Password, loginRequest.IsEmployee);

            if (isValid)
            {
                var cookieResult = await SetCookie(loginRequest);

                if (cookieResult is BadRequestObjectResult)
                {
                    return cookieResult;
                }
                
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

