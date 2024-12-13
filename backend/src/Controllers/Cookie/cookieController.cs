namespace WPR.Controllers.Cookie;

using Microsoft.AspNetCore.Mvc;
using WPR.Database;
using System;
using WPR.Repository;
using MySqlX.XDevAPI.Common;
using WPR.Cryption;

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


    [HttpGet("GetUserName")]
    public async Task<IActionResult> GetUserName()
    {
        string loginCookie = HttpContext.Request.Cookies["LoginSession"];

        if(string.IsNullOrEmpty(loginCookie))
        {
            Console.WriteLine("No cookie");
            return BadRequest(new { message = "No Cookie"});
        }
        
        try
        {
            string userName = await _userRepository.GetUserNameAsync(_crypt.Decrypt(loginCookie));
            Console.WriteLine(userName);
            return Ok(new { message = userName });
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return BadRequest(new { message = "User Not Found"});
        }
    }

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
            (bool status, string message) kindEmployee = await _userRepository.GetKindEmployeeAsync(_crypt.Decrypt(employeeCookie));

            if (kindEmployee.status)
            {
                Console.WriteLine(kindEmployee.message);
                return Ok(new { kindEmployee.message });
            }
            
            return BadRequest(new { message = "No office found" });
        } 
        catch
        {
            Response.Cookies.Append("LoginSessionEmployee", "Invalid cookie", new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                Expires = DateTimeOffset.UtcNow.AddDays(-1)
            });
            
            return BadRequest(new { message = "Invalid Cookie, new cookie set" });
        }
    }
}