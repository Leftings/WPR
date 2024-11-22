namespace WPR.Controllers.Home;

using Microsoft.AspNetCore.Mvc;
using WPR.Database;
using System;
using WPR.Repository;
using MySqlX.XDevAPI.Common;

[Route("api/[controller]")]
[ApiController]
public class HomeController : ControllerBase
{
    private readonly Connector _connector;
    private readonly IUserRepository _userRepository;

    public HomeController(Connector connector, IUserRepository userRepository)
    {
        _connector = connector ?? throw new ArgumentNullException(nameof(connector));
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
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