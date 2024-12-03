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

    public ChangeUserSettingsController(Connector connector, IUserRepository userRepository)
    {
        _connector = connector ?? throw new ArgumentNullException(nameof(connector));
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
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
}