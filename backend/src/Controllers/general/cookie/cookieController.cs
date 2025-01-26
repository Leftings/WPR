namespace WPR.Controllers.General.Cookie;

using Microsoft.AspNetCore.Mvc;
using WPR.Database;
using System;
using WPR.Repository;
using MySqlX.XDevAPI.Common;
using WPR.Cryption;

/// <summary>
/// CookieController is de controller voor het aanmaken, verwijderen en ondervragen van cookies
/// </summary>
[Route("api/[controller]")]
[ApiController]
public class CookieController : ControllerBase
{
    private readonly IConnector _connector;
    private readonly IUserRepository _userRepository;
    private readonly Crypt _crypt;

    public CookieController(IConnector connector, IUserRepository userRepository, Crypt crypt)
    {
        _connector = connector ?? throw new ArgumentNullException(nameof(connector));
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _crypt = crypt ?? throw new ArgumentNullException(nameof(crypt));
    }

    /// <summary>
    /// De geencrypted user id wordt uit de cookie gepakt en wordt vervolgens gedecrypt doorgestuurd
    /// </summary>
    /// <returns></returns>
    [HttpGet("GetUserId")]
    public async Task<IActionResult> GetUserId()
    {
        string loginCookie = HttpContext.Request.Cookies["LoginSession"];
        string? loginCookie2 = HttpContext.Request.Cookies["LoginVehicleManagerSession"];
        string? loginCookie3 = HttpContext.Request.Cookies["LoginEmployeeSession"];

        try
        {
            if (!string.IsNullOrEmpty(loginCookie))
            {
                return Ok(new { message = _crypt.Decrypt(loginCookie) });
            }
            if (!string.IsNullOrEmpty(loginCookie2))
            {
                return Ok(new { message = _crypt.Decrypt(loginCookie2) });
            }
            if (!string.IsNullOrEmpty(loginCookie3))
            {
                return Ok(new { message = _crypt.Decrypt(loginCookie3)} );
            }

            return BadRequest(new { message = "No Cookie" });

        }
        catch
        {
            // Ongeldige cookies worden verwijderd
            Response.Cookies.Append("LoginSession", "Invalid cookie", new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                Expires = DateTimeOffset.UtcNow.AddDays(-1)
            });

            Response.Cookies.Append("LoginEmployeeSession", "Invalid cookie", new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                Expires = DateTimeOffset.UtcNow.AddDays(-1)
            });

            Response.Cookies.Append("LoginVehicleManagerSession", "Invalid cookie", new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                Expires = DateTimeOffset.UtcNow.AddDays(-1)
            });
            
            return BadRequest(new { message = "Invalid Cookie, new cookie set" });
        }
    }

    /// <summary>
    /// De gebruikersnaam wordt via de user id opgehaald via de backend
    /// </summary>
    /// <returns></returns> 
    [HttpGet("GetUserName")]
    public async Task<IActionResult> GetUserName()
    {
        string loginCookie = HttpContext.Request.Cookies["LoginSession"];

        if(string.IsNullOrEmpty(loginCookie))
        {
            return BadRequest(new { message = "No Cookie"});
        }
        
        try
        {
            string userName = await _userRepository.GetUserNameAsync(_crypt.Decrypt(loginCookie));
            return Ok(new { message = userName });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = $"{ex.Message}\nUser Not Found"});
        }
    }

    /// <summary>
    /// De loginsession cookie wordt veranderd naar een cookie met een negatieve tijd, zodat de cookie expires
    /// </summary>
    /// <returns></returns>
    [HttpPost("Logout")]
    public IActionResult Logout()
    {
        if (Request.Cookies["LoginSession"] != null)
        {
            Response.Cookies.Append("LoginSession", "", new CookieOptions {Expires = DateTimeOffset.UtcNow.AddDays(-1)});
            Console.WriteLine("Cookie cleared");            
        }
        else
        {
            Console.WriteLine("No cookie found");
        }
        return Ok(new { message = "User Logged Out"});
    }

    /// <summary>
    /// Het soort medewerker wordt uit de cookie opgehaald, door middel van de user id, zodat de juiste soort medewerker naar de frontend verstuurd wordt
    /// </summary>
    /// <returns></returns>
    [HttpGet("GetKindEmployee")]
    public async Task<IActionResult> GetKindEmployeeAsync()
    {
        string employeeCookie = HttpContext.Request.Cookies["LoginEmployeeSession"];
        Console.WriteLine(employeeCookie);

        if (string.IsNullOrEmpty(employeeCookie))
        {
            return BadRequest(new { message = "No Cookie" });
        }

        try
        {
            // Updated to match the new tuple
            (bool status, string message, string officeType) kindEmployee = 
                await _userRepository.GetKindEmployeeAsync(_crypt.Decrypt(employeeCookie));

            if (kindEmployee.status)
            {
                Console.WriteLine(kindEmployee.message);
                return Ok(new 
                { 
                    message = kindEmployee.message, 
                    officeType = kindEmployee.officeType 
                });
            }
        
            return BadRequest(new { message = "No office found for this employee" });
        } 
        catch
        {
            // Remove invalid cookie and return a response
            Response.Cookies.Append("LoginSessionEmployee", "Invalid cookie", new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                Expires = DateTimeOffset.UtcNow.AddDays(-1)
            });
        
            return BadRequest(new { message = "Invalid Cookie, new cookie set" });
        }
    }

    /// <summary>
    /// Er wordt gekeken of de Vehicle Manager bestaat.
    /// Ongeldige cookies worden verwijderd.
    /// </summary>
    /// <returns></returns>
    [HttpGet("IsVehicleManager")]
    public async Task<IActionResult> IsVehicleManagerAsync()
    {
        string? loginCookie2 = HttpContext.Request.Cookies["LoginVehicleManagerSession"];

        Console.WriteLine(_crypt.Decrypt(loginCookie2));


        try
        {
            if (!string.IsNullOrEmpty(loginCookie2))
            {
                return Ok(new {data = _crypt.Decrypt(loginCookie2)});
            }

            return BadRequest(new { message = "No Cookie" });
        }
        catch
        {

            Response.Cookies.Append("LoginVehicleManagerSession", "Invalid cookie", new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                Expires = DateTimeOffset.UtcNow.AddDays(-1)
            });
            
            return BadRequest(new { message = "Invalid Cookie, new cookie set" });
        }
    }
}