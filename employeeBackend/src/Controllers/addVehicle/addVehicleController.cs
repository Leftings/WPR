namespace Employee.Controllers.AddVehicle;

using Microsoft.AspNetCore.Mvc;
using Employee.Database;
using System;
using MySqlX.XDevAPI.Common;

[Route("api/[controller]")]
[ApiController]
public class CookieController : ControllerBase
{
    private readonly Connector _connector;
    private readonly IUserRepository _userRepository;

    public CookieController(Connector connector, IUserRepository userRepository)
    {
        _connector = connector ?? throw new ArgumentNullException(nameof(connector));
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
    }

    [HttpGet("GetEmployeeId")]
    public async Task<IActionResult> GetEmployeeIdAsync()
    {
        string loginCookie = HttpContext.Request.Cookies["LoginSession"];

        if (string.IsNullOrEmpty(loginCookie))
        {
            return BadRequest(new { message = "No Cookie" });
        }

        try
        {
            return Ok(new { message = _crypt.Decrypt(loginCookie) });
        }
        catch
        {
            Response.Cookies.Append("LoginSession", "Invalid cookie", new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                Expires = DateTimeOffset.UtcNow.AddDays(-1)
            });
            
            return BadRequest(new { message = "Invalid Cookie, new cookie set" });
        }
    }

}