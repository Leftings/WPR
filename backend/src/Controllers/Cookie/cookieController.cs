namespace WPR.Controllers.Cookie;

using Microsoft.AspNetCore.Mvc;
using WPR.Database;
using System;
using WPR.Repository;
using MySqlX.XDevAPI.Common;
using WPR.Cryption;
using WPR.Controllers.Cookie;

[Route("api/[controller]")]
[ApiController]
public class CookieController : ControllerBase
{
    private readonly IUserRepository _userRepository;
    private readonly Crypt _crypt;

    public CookieController(IUserRepository userRepository, Crypt crypt)
    {
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _crypt = crypt ?? throw new ArgumentNullException(nameof(crypt));
    }

    [HttpGet("GetUserId")]
    public async Task<IActionResult> GetUserIdAsync()
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
    public async Task<IActionResult> GetUserNameAsync()
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
}