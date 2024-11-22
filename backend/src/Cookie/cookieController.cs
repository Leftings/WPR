namespace WPR.Controllers.Cookie;

using Microsoft.AspNetCore.Mvc;
using WPR.Database;
using System;
using WPR.Repository;
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

    [HttpGet("GetUserId")]
    public async Task<IActionResult> GetUserId()
    {
        string loginCookie = HttpContext.Request.Cookies["LoginSession"];

        if(string.IsNullOrEmpty(loginCookie))
        {
            return BadRequest(new { message = "No Cookie"});
        }

        Console.WriteLine(loginCookie);

        return Ok(new { message = loginCookie});
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

        var connection = _connector.CreateDbConnection();

        try
        {
            string userName = await _userRepository.GetUserNameAsync(connection, loginCookie);
            Console.WriteLine(userName);
            return Ok(new { message = userName });
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return BadRequest(new { message = "User Not Found"});
        }
    }
}