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
    public async Task<IActionResult> ChangeUserInfo([FromBody] ChangeUserRequest changeUserRequest)
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

        var connection = _connector.CreateDbConnection();

        if (await _userRepository.EditUserInfoAsync(data))
        {
            connection.Close();

            return Ok(new {message = "Data Updated"});
        }

        connection.Close();

        return BadRequest(new {message = "A Problem Occured Updating The Data"});
    }
}