using WPR.Cryption;

namespace WPR.Controllers.Customer.ChangeUserSettings;

using Microsoft.AspNetCore.Mvc;
using WPR.Database;
using System;
using WPR.Repository;
using MySqlX.XDevAPI.Common;
using WPR.Repository.DatabaseCheckRepository;

/// <summary>
/// ChangeUserSettingsController is de controller die aanvragen van buitenaf ontvangt en vervolgens gegevens uit de backend ophaald een terug geeft 
/// </summary>
[Route("api/[controller]")]
[ApiController]
public class ChangeUserSettingsController : ControllerBase
{
    private readonly IConnector _connector;
    private readonly IUserRepository _userRepository;
    private readonly Crypt _crypt;
    private readonly IDatabaseCheckRepository _databaseCheckRepository;

    public ChangeUserSettingsController(IConnector connector, IUserRepository userRepository, Crypt crypt, IDatabaseCheckRepository databaseCheckRepository)
    {
        _connector = connector ?? throw new ArgumentNullException(nameof(connector));
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _crypt = crypt ?? throw new ArgumentNullException(nameof(crypt));
        _databaseCheckRepository = databaseCheckRepository ?? throw new ArgumentNullException(nameof(databaseCheckRepository));
    }

    /// <summary>
    /// Gewijzigde gegevens worden gefilter van de ongewijzigde gegevens en worden verstuurd naar de backend
    /// </summary>
    /// <param name="changeUserRequest"></param>
    /// <returns></returns>
    [HttpPut("ChangeUserInfo")]
    public async Task<IActionResult> ChangeUserInfoAsync([FromBody] ChangeUserRequest changeUserRequest)
    {
        if (changeUserRequest == null)
        {
            return BadRequest("Body can not be null");
        }

        List<object[]> data = new List<object[]>();

        // gegevens worden uit de lijst gehaald (naam van de collom, de nieuwe waarde, soort waarde)
        foreach (var propertyInfo in typeof(ChangeUserRequest).GetProperties())
        {
            var propertyName = propertyInfo.Name;
            var propertyValue = propertyInfo.GetValue(changeUserRequest);
            var propertyType = propertyInfo.PropertyType;

            if (!propertyValue.Equals(""))
            {
                data.Add(new object[] {propertyName, propertyValue, propertyType});
            }
        }

        var updated = await _userRepository.EditUserInfoAsync(data);
        if (updated.status)
        {
            return Ok( new ChangeUserSettingsResponse {Message = "Data Updated"});
        }

        return BadRequest(new ChangeUserSettingsErrorResponse {Message = updated.message});
    }

    /// <summary>
    /// Gebruikers kunnen hun account, samen met hun gegevens, verwijderen uit het systeem
    /// </summary>
    /// <returns>Deleted account</returns>
    [HttpDelete("DeleteUser")]
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

            var deleteUser = _databaseCheckRepository.DeleteUser(Convert.ToInt32(decryptedLoginCookie));
            Response.Cookies.Append("LoginSession", "", new CookieOptions { Expires = DateTimeOffset.Now.AddDays(-1) });
            return StatusCode(deleteUser.StatusCode, new { message = deleteUser.Message });
        }
        catch (OverflowException ex)
        {
            Console.WriteLine(ex.Message);
            throw;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}
