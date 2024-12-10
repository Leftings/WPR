using WPR.Cryption;

namespace WPR.Controllers.ChangeUserSettings;

using Microsoft.AspNetCore.Mvc;
using WPR.Database;
using System;
using WPR.Repository;
using MySqlX.XDevAPI.Common;

[Route("api/[controller]")]
[ApiController]
public class ChangeUserSettingsController : ControllerBase
{
    private readonly Connector _connector;
    private readonly IUserRepository _userRepository;
    private readonly Crypt _crypt;

    public ChangeUserSettingsController(Connector connector, IUserRepository userRepository, Crypt crypt)
    {
        _connector = connector ?? throw new ArgumentNullException(nameof(connector));
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _crypt = crypt ?? throw new ArgumentNullException(nameof(crypt));
    }

    [HttpPut("ChangeUserInfo")]
    public async Task<IActionResult> ChangeUserInfoAsync([FromBody] ChangeUserRequest changeUserRequest)
    {
        if (changeUserRequest == null)
        {
            return BadRequest("Body can not be null");
        }

        List<object[]> data = new List<object[]>();

        foreach (var propertyInfo in typeof(ChangeUserRequest).GetProperties())
        {
            var propertyName = propertyInfo.Name;
            var propertyValue = propertyInfo.GetValue(changeUserRequest);
            var propertyType = propertyInfo.PropertyType;

            if (!propertyValue.Equals(""))
            {
                Console.WriteLine($"{propertyName} {propertyValue} {propertyType}");
                data.Add(new object[] {propertyName, propertyValue, propertyType});
            }
        }

        var updated = await _userRepository.EditUserInfoAsync(data);
        if (updated.status)
        {
            return Ok(new {message = "Data Updated"});
        }

        Console.WriteLine(updated.message);
        Console.WriteLine(updated.message.Length);

        return BadRequest(new {updated.message});
    }

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