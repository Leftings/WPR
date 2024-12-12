namespace Employee.Controllers.acceptHireRequest;

using Microsoft.AspNetCore.Mvc;
using Employee.Database;
using System;
using MySqlX.XDevAPI.Common;
using Employee.Repository;
using MySql.Data.MySqlClient;
using Employee.Cryption;

[Route("api/[controller]")]
[ApiController]
public class AcceptHireRequestController : ControllerBase
{
    private readonly Connector _connector;
    private readonly IUserRepository _userRepository;
    private readonly Crypt _crypt;

    public AcceptHireRequestController(Connector connector, IUserRepository userRepository, Crypt crypt)
    {
        _connector = connector ?? throw new ArgumentNullException(nameof(connector));
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _crypt = crypt ?? throw new ArgumentNullException(nameof(crypt));
    }

    [HttpPatch("answerHireRequest")]
    public async Task<IActionResult> AnswerHireRequestAsync([FromForm] acceptHireRequestRequest request)
    {
        return Ok();
    }

    [HttpGet("GetInProgressFromUser")]
    public async Task<IActionResult> GetInProgressFromUserAsync()
    {
        var inProgress = await _userRepository.GetInReviewFromUserAsync(GetCurrentUserId());

        return Ok(new { message = inProgress.data});
    }

    [HttpGet("getAllRequests")]
    public async Task<IActionResult> GetAllRequestsAsync()
    {
        var available = await _userRepository.GetRequestedAsync();
        return Ok(new { message = available.data });
    }

    private string GetCurrentUserId()
    {
        string loginCookie = HttpContext.Request.Cookies["LoginEmployeeSession"];

        if (string.IsNullOrEmpty(loginCookie))
        {
            throw new Exception("No Cookie");
        }

        try
        {
            return _crypt.Decrypt(loginCookie);
        }
        catch
        {
            Response.Cookies.Append("LoginEmployeeSession", "Invalid cookie", new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                Expires = DateTimeOffset.UtcNow.AddDays(-1)
            });

            throw new Exception("Invalid Cookie");
        }
    }
}