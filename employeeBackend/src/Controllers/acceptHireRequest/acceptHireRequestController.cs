namespace Employee.Controllers.acceptHireRequest;

using Microsoft.AspNetCore.Mvc;
using Employee.Database;
using System;
using MySqlX.XDevAPI.Common;
using Employee.Repository;
using MySql.Data.MySqlClient;
using Employee.Cryption;

/// <summary>
/// AcceptHireRequestController zorgt ervoor dat alle huuraanvragen verzameld worden en bestempelt kunnen worden als geacepteerd of geweigerd
/// </summary>
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

    /// <summary>
    /// Alle ids van de aanvragen worden opgehaald
    /// </summary>
    /// <param name="user"></param>
    /// <returns></returns>
    [HttpGet("getReviewsIds")]
    public async Task<IActionResult> GetReviewsIdsAsync(string user)
    {
        var ids = await _userRepository.GetReviewIdsAsync(user, GetCurrentUserId());
        return Ok(new { message = ids.ids});
    }

    /// <summary>
    /// De gegevens van een specifieke aanvraag worden opgehaald
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("getReview")]
    public async Task<IActionResult> GetReviewAsync(string id)
    {
        var review = await _userRepository.GetReviewAsync(id);
        return Ok(new { message = review.data });
    }

    /// <summary>
    /// Een aanvraag kan beantwoord worden
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPatch("answerHireRequest")]
    public async Task<IActionResult> AnswerHireRequestAsync([FromBody] HireRequest request)
    {
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

    /// <summary>
    /// Huidige user id wordt opgehaald, voor validatie en logging
    /// </summary>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    private string GetCurrentUserId()
    {
        // Alleen de cookie van Employee of Vehicle Manager bestaat
        string loginCookie = HttpContext.Request.Cookies["LoginEmployeeSession"];
        string loginCookie2 = HttpContext.Request.Cookies["LoginVehicleManagerSession"];

        try
        {
            if (!string.IsNullOrEmpty(loginCookie))
            {
                return _crypt.Decrypt(loginCookie);
            }
            else if (!string.IsNullOrEmpty(loginCookie2))
            {
                return _crypt.Decrypt(loginCookie2);
            }
            else
            {
                throw new Exception("No (valid) cookie");
            }
        }
        catch
        {
            // Ongeldig cookie wordt meteen verwijderd
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