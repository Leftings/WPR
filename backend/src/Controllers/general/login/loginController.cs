namespace WPR.Controllers.General.Login;

using Microsoft.AspNetCore.Mvc;
using WPR.Repository;
using MySql.Data.MySqlClient;
using System;
using WPR.Database;
using WPR.Cryption;
using WPR.Controllers.General.Cookie;

/// <summary>
/// LoginController zorgt ervoor dat een gebruiker kan inloggen, daarnaast wordt er ook gekeken of de gebruiker een geldige cookie heeft
/// </summary>
[Route("api/[controller]")]
[ApiController]
public class LoginController : ControllerBase
{
    private readonly IUserRepository _userRepository;
    private readonly IConnector _connector;
    private readonly SessionHandler _sessionHandler;
    private readonly Crypt _crypt;

    public LoginController(IUserRepository userRepository, IConnector connector, SessionHandler sessionHandler, Crypt crypt)
    {
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _connector = connector ?? throw new ArgumentNullException(nameof(connector));
        _sessionHandler = sessionHandler ?? throw new ArgumentNullException(nameof(sessionHandler));
        _crypt = crypt ?? throw new ArgumentNullException(nameof(crypt));
    }

    /// <summary>
    /// Op het moment van inloggen worden alle cookies van mogelijke gebruikers verwijderd
    /// </summary>
    private void RemoveOldCookieAsync()
    {
        try
        {
            if (!string.IsNullOrEmpty(Request.Cookies["LoginEmployeeSession"]))
            {
                _sessionHandler.CreateInvalidCookie(Response.Cookies, "LoginEmployeeSession");
            }
            
            if (!string.IsNullOrEmpty(Request.Cookies["LoginVehicleManagerSession"]))
            {
                _sessionHandler.CreateInvalidCookie(Response.Cookies, "LoginVehicleManagerSession");
            }
        } 
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }

    /// <summary>
    /// Een nieuw cookie wordt gezet, met de juiste cookie naa men waarders
    /// </summary>
    /// <param name="loginRequest"></param>
    /// <returns></returns>
    private async Task<IActionResult> SetCookieAsync(LoginRequest loginRequest)
    {
        RemoveOldCookieAsync();
        var connection = _connector.CreateDbConnection();

        try
        {
            // De table voor de gebruiker wordt vastgesteld
            string table;

            if (loginRequest.UserType.Equals("Employee"))
            {
                table = "Staff";
            }
            else if (loginRequest.UserType.Equals("Customer"))
            {
                table = "Customer";
            }
            else
            {
                table = "VehicleManager";
            }

            // De user id wordt opgehaald, doormiddel van de meegegeven email en jusite tabel
            string userId = await _userRepository.GetUserIdAsync(loginRequest.Email, table);

            if (userId.Equals("No user found"))
            {
                return BadRequest(new { message = "User ID not found." });
            }
            
            // De juiste cookie wordt aangemaakt, doormiddel van het gebruikers type die meegegeven wordt via de inlog request
            if (loginRequest.UserType.Equals("Employee"))
            {
                _sessionHandler.CreateCookie(Response.Cookies, "LoginEmployeeSession", _crypt.Encrypt(userId.ToString()));
            }
            else if (loginRequest.UserType.Equals("Customer"))
            {
                _sessionHandler.CreateCookie(Response.Cookies, "LoginSession", _crypt.Encrypt(userId.ToString()));
            }
            else
            {
                _sessionHandler.CreateCookie(Response.Cookies, "LoginVehicleManagerSession", _crypt.Encrypt(userId.ToString()));
            }
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        finally
        {
            connection.Close();
        }

        return Ok();
    }

    /// <summary>
    /// De session voor de customer wordt gecheckt
    /// </summary>
    /// <returns></returns>
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

    /// <summary>
    /// De session voor de Car and All medewerker wordt gecheckt
    /// </summary>
    /// <returns></returns>
    [HttpGet("CheckSessionStaff")]
    public async Task<IActionResult> CheckSessionStaff()
    {
        string sessionValue = Request.Cookies["LoginEmployeeSession"];
    
        if (!string.IsNullOrEmpty(sessionValue))
        {
            try
            {
                string userId = _crypt.Decrypt(sessionValue);

                (bool status, string message, string officeType) employeeInfo = await _userRepository.GetKindEmployeeAsync(userId);

                if (employeeInfo.status)
                {
                    return Ok(new 
                    {
                        message = "Session active",
                        sessionValue,
                        officeType = employeeInfo.officeType
                    });
                }

                return Unauthorized(new { message = "Session active but office type not found" });
            }
            catch
            {
                return Unauthorized(new { message = "Session is invalid or cannot be decrypted" });
            }
        }

        return Unauthorized(new { message = "Session expired or is not found" });
    }

    /// <summary>
    /// De session voor de Vehicle Manager wordt gecheckt
    /// </summary>
    /// <returns></returns>
    [HttpGet("CheckSessionVehicleManager")]
    public async Task<IActionResult> CheckSessionVehicleManager()
    {
        string sessionValue = Request.Cookies["LoginVehicleManagerSession"];
        if (!string.IsNullOrEmpty(sessionValue))
        {
            return Ok( new {message = "session active ", sessionValue});
        }
        return Unauthorized(new { message = "Session expired or is not found" });
    }

    /// <summary>
    /// Een gebruiker wordt ingelogt
    /// </summary>
    /// <param name="loginRequest"></param>
    /// <returns></returns>
    [HttpPost("login")]
    public async Task <IActionResult> LoginAsync([FromBody] LoginRequest loginRequest)
    {
        // Er wordt gekeken of de loginrequest niet null is en of er een emailadres en wachtwoord is ingevuld
        if (loginRequest == null || string.IsNullOrEmpty(loginRequest.Email) || string.IsNullOrEmpty(loginRequest.Password))
        {
            return BadRequest(new { message = "Invalid input. Please provide email and password." });
        }

        try
        {
            // Er wordt gekeken of het emailadres en wachtwoord overeenkomt in de juiste tabel (UserCustomer, Staff of VehicleManager)
            bool isValid = await _userRepository.ValidateUserAsync(loginRequest.Email, loginRequest.Password, loginRequest.UserType);

            if (isValid)
            {
                // Een nieuw cookie wordt aangemaakt
                var cookieResult = await SetCookieAsync(loginRequest);

                if (cookieResult is BadRequestObjectResult)
                {
                    return cookieResult;
                }
                
                return Ok(new { message = "Login Successful." });
            }
            else
            {
                // De gegevens kwamen niet overeen met wat er in de DataBase stond en wordt geweigerd
                return Unauthorized(new { message = "Login Failed." });
            }
        }
        catch (MySqlException ex)
        {
            return StatusCode(500, new { message = ex.Message});
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }
}
