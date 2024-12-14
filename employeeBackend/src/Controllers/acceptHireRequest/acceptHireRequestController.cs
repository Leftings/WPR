namespace Employee.Controllers.acceptHireRequest;

using Microsoft.AspNetCore.Mvc;
using Employee.Database;
using System;
using MySqlX.XDevAPI.Common;
using Employee.Repository;
using MySql.Data.MySqlClient;
using Employee.Cryption;

[ApiController]
[Route("api/[controller]")]
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

    [HttpGet("getReviewsIds")]
    public async Task<IActionResult> GetReviewsIdsAsync(string user)
    {
        var ids = await _userRepository.GetReviewIdsAsync(user, GetCurrentUserId());
        return Ok(new { message = ids.ids});
    }

    [HttpGet("getReview")]
    public async Task<IActionResult> GetReviewAsync(string id)
    {
        var review = await _userRepository.GetReviewAsync(id);

        foreach (var x in review.data)
        {
            Console.WriteLine(x);
        }

        return Ok(new { message = review.data });
    }

    [HttpPatch("answerHireRequest")]
    public async Task<IActionResult> AnswerHireRequestAsync([FromBody] HireRequest request)
    {
        Console.WriteLine($"Received Request - Id: {request.Id}, Status: {request.Status}");

        if (request == null)
        {
            return BadRequest(new { message = "Invalid request body" });
        }

        var setStatus = await _userRepository.SetStatusAsync(request.Id.ToString(), request.Status, GetCurrentUserId(), request.userType);

        if (setStatus.status)
        {
            return Ok(new { message = setStatus.message });
        }

        return BadRequest(new { message = setStatus.message });
    }


    private string GetCurrentUserId()
    {
        string loginCookie = HttpContext.Request.Cookies["LoginEmployeeSession"];
        string loginCookie2 = HttpContext.Request.Cookies["LoginVehicleManagerSession"];

        try
        {
            if (!string.IsNullOrEmpty(loginCookie))
            {
                Console.WriteLine(loginCookie);
                Console.WriteLine(_crypt.Decrypt(loginCookie));
                return _crypt.Decrypt(loginCookie);
            }
            else if (!string.IsNullOrEmpty(loginCookie2))
            {
                Console.WriteLine(loginCookie2);
                return _crypt.Decrypt(loginCookie2);
            }
            else
            {
                throw new Exception("No (valid) cookie");
            }
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