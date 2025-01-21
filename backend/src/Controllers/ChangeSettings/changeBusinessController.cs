using Microsoft.AspNetCore.Mvc;
using WPR.Cryption;
using WPR.Repository;
using WPR.Utils;

namespace WPR.Controllers.ChangeBusinessSettings;

[Route("api/[controller]")]
[ApiController]
public class ChangeBusinessSettings : ControllerBase
{
    private readonly IUserRepository _userRepository;
    private readonly Crypt _crypt;

    public ChangeBusinessSettings(IUserRepository userRepository, Crypt crypt)
    {
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _crypt = crypt ?? throw new ArgumentNullException(nameof(crypt));
    }

    [HttpPut("ChangeBusinessInfo")]
    public async Task<IActionResult> ChangeBusinessInfoAsync([FromBody] ChangeBusinessRequest request)
    {
        Console.WriteLine("X");
        if (request == null)
        {
            return BadRequest("Body can not be null");
        }

        if (!string.IsNullOrEmpty(request.VehicleManagerInfo.Password))
        {
            (bool Valid, string Message) result = PasswordChecker.IsValidPassword(request.VehicleManagerInfo.Password);

            if (!result.Valid)
            {
                return StatusCode(412, new { message = result.Message });
            }

        }

        (int StatusCode, string Message) updated = await _userRepository.ChangeBusinessInfo(request);
        
        return StatusCode(updated.StatusCode, new { message = updated.Message} );
    }

    /// <summary>
    /// Gebruikers kunnen hun account, samen met hun gegevens, verwijderen uit het systeem
    /// </summary>
    /// <returns></returns>
    [HttpDelete("DeleteBusiness")]
    public async Task<IActionResult> DeleteUserAsync() {
        
        string loginCookie = HttpContext.Request.Cookies["LoginSession"];
        
        if(string.IsNullOrEmpty(loginCookie))
        {
            Console.WriteLine("No cookie");
            return BadRequest(new { message = "No Cookie"});
        }
        
        Console.WriteLine(loginCookie);

        try
        {
            string decryptedLoginCookie = _crypt.Decrypt(loginCookie);
            Console.WriteLine(decryptedLoginCookie);
            var result = await _userRepository.DeleteUserAsync(decryptedLoginCookie);
                if (result.status)
                {
                    Response.Cookies.Append("LoginSession", "", new CookieOptions { Expires = DateTimeOffset.Now.AddDays(-1) });
                    Console.WriteLine("Cookie cleared");
                    return Ok(new {message = result.message});
                }
                return BadRequest(new {message = result.message});
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
        
    }
}

