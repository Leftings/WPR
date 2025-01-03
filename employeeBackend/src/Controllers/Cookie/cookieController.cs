namespace Employee.Controllers.Cookie;

using Microsoft.AspNetCore.Mvc;
using Employee.Database;
using System;
using Employee.Repository;
using MySqlX.XDevAPI.Common;
using Employee.Cryption;

/// <summary>
/// Controller voor het aanmaken en ondervragen van cookies
/// </summary>
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

    /// <summary>
    /// User id wordt opgevraagd vanuit cookie.
    /// Als de cookie ongeldig is, wordt deze verwijderd.
    /// </summary>
    /// <returns></returns>
    [HttpGet("GetUserId")]
    public async Task<IActionResult> GetUserId()
    {
        // Employee cookie en Vehicle Manager cookie kunnen niet tergelijktijd bestaan
        string? loginCookie = HttpContext.Request.Cookies["LoginEmployeeSession"];
        string? loginCookie2 = HttpContext.Request.Cookies["LoginVehicleManagerSession"];

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
            // Ongeldige cookies worden verwijderd
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
    /// Er wordt gekeken of de Vehicle Manager bestaat.
    /// Ongeldige cookies worden verwijderd.
    /// </summary>
    /// <returns></returns>
    [HttpGet("IsVehicleManager")]
    public async Task<IActionResult> IsVehicleManagerAsync()
    {
        string? loginCookie2 = HttpContext.Request.Cookies["LoginVehicleManagerSession"];

        string decryptedCookie = _crypt.Decrypt(loginCookie2);


        try
        {
            if (!string.IsNullOrEmpty(loginCookie2))
            {
                return Ok(true);
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