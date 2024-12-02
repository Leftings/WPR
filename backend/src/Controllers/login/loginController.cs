namespace WPR.Controllers.Login;

using Microsoft.AspNetCore.Mvc;
using WPR.Repository;
using MySql.Data.MySqlClient;
using System;
using WPR.Controllers.Cookie;
using WPR.Database;
using WPR.Cryption;
using WPR.Hashing;

[Route("api/[controller]")]
[ApiController]
public class LoginController : ControllerBase
{
    private readonly IUserRepository _userRepository;
    private readonly Connector _connector;
    private readonly SessionHandler _sessionHandler;
    private readonly Crypt _crypt;
    private readonly Hash _hash;

    public LoginController(IUserRepository userRepository, Connector connector, SessionHandler sessionHandler, Crypt crypt, Hash hash)
    {
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _connector = connector ?? throw new ArgumentNullException(nameof(connector));
        _sessionHandler = sessionHandler ?? throw new ArgumentNullException(nameof(sessionHandler));
        _crypt = crypt ?? throw new ArgumentNullException(nameof(crypt));
    }

    private async Task<IActionResult> SetCookieAsync(LoginRequest loginRequest)
    {
        var connection = _connector.CreateDbConnection();

        try
        {
            int userId = await _userRepository.GetUserIdAsync(loginRequest.Email);

            if (userId <= 0)
            {
                return BadRequest(new { message = "User ID not found." });
            }
            
            _sessionHandler.CreateCookie(Response.Cookies, "LoginSession", _crypt.Encrypt(userId.ToString()));

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

    [HttpGet("CheckSession")]
    public IActionResult CheckSession()
    {
        string sessionValue = Request.Cookies["LoginSession"];
        
        if (!string.IsNullOrEmpty(sessionValue))
        {
            return Ok( new {message = "session active ", sessionValue});
        }
        return Unauthorized(new { message = "Session expired or is not found" });
    }

    [HttpPost("login")]
    public async Task <IActionResult> LoginAsync([FromBody] LoginRequest loginRequest)
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
                var cookieResult = await SetCookieAsync(loginRequest);

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
