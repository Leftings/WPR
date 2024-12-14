namespace Employee.Controllers.Cookie;

using Microsoft.AspNetCore.Mvc;
using Employee.Database;
using System;
using Employee.Repository;
using MySqlX.XDevAPI.Common;
using Employee.Cryption;

[Route("api/[controller]")]
[ApiController]
public class CookieController : ControllerBase
{
    private readonly Connector _connector;
    private readonly IUserRepository _userRepository;
    private readonly Crypt _crypt;

    public CookieController(Connector connector, IUserRepository userRepository, Crypt crypt)
    {
        _connector = connector ?? throw new ArgumentNullException(nameof(connector));
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _crypt = crypt ?? throw new ArgumentNullException(nameof(crypt));
    }

    [HttpGet("GetUserId")]
    public async Task<IActionResult> GetUserId()
    {
        string? loginCookie = HttpContext.Request.Cookies["LoginEmployeeSession"];
        string? loginCookie2 = HttpContext.Request.Cookies["LoginVehicleManagerSession"];

        Console.WriteLine(loginCookie2);

        Console.WriteLine(_crypt.Decrypt(loginCookie2) + "  XXXXXXXXXXXXXX");

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

            return BadRequest(new { message = "No Cookie" });

        }
        catch
        {
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
}