using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using WPR.Repository;
using WPR.Repository.DatabaseCheckRepository;
using WPR.Utils;

namespace WPR.Controllers.Employee.VehicleManager.ChangeBusinessSettings;

[Route("api/[controller]")]
[ApiController]
public class ChangeBusinessSettingsController : ControllerBase
{
    private readonly IUserRepository _userRepository;
    private readonly IEmployeeRepository _employeeRepository;
    private readonly IDatabaseCheckRepository _databaseCheckRepository;

    public ChangeBusinessSettingsController(IUserRepository userRepository, IEmployeeRepository employeeRepository,
        IDatabaseCheckRepository databaseCheckRepository)
    {
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _employeeRepository = employeeRepository ?? throw new ArgumentNullException(nameof(employeeRepository));
        _databaseCheckRepository =
            databaseCheckRepository ?? throw new ArgumentNullException(nameof(databaseCheckRepository));
    }

    [HttpGet("GetBusinessInfo")]
    public async Task<IActionResult> GetBusinessInfoAsync(int id)
    {
        try
        {
            Console.WriteLine($"Fetching KvK for VehicleManager ID: {id}");
            (int StatusCode, string Message, int KvK) kvkResponse = _employeeRepository.GetKvK(id);

            if (kvkResponse.StatusCode == 200)
            {
                Console.WriteLine($"KvK retrieved: {kvkResponse.KvK}");
                (bool Status, string Message, Dictionary<string, object> Data) businessInfo =
                    _employeeRepository.GetBusinessInfo(kvkResponse.KvK);

                if (businessInfo.Status)
                {
                    return StatusCode(200, new { data = businessInfo.Data });
                }
                else
                {
                    return BadRequest(new { message = businessInfo.Message });
                }
            }

            return StatusCode(kvkResponse.StatusCode, kvkResponse.Message);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            return StatusCode(500, new { message = ex.Message });
        }
    }

    [HttpPut("ChangeBusinessInfo")]
    public async Task<IActionResult> ChangeBusinessInfoAsync([FromBody] ChangeBusinessRequest request)
    {
        if (request == null)
        {
            return BadRequest("Body cannot be null");
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

        return StatusCode(updated.StatusCode, new { message = updated.Message });
    }

    /// <summary>
    /// Gebruikers kunnen hun account, samen met hun gegevens, verwijderen uit het systeem
    /// </summary>
    /// <returns></returns>
    [HttpDelete("DeleteBusiness")]
    public async Task<IActionResult> DeleteUserAsync([FromBody] DeleteBusinessRequest request)
    {
        var deleteResponse = _databaseCheckRepository.DeleteBusiness(request.KvK);

        if (deleteResponse.StatusCode == 200)
        {
            Response.Cookies.Append("LoginVehicleManagerSession", "Invalid cookie", new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                Expires = DateTimeOffset.UtcNow.AddDays(-1)
            });
        }

        return StatusCode(deleteResponse.StatusCode, new { message = deleteResponse.Message });
    }

    [HttpDelete("DeleteVehicleManager")]
    public async Task<IActionResult> DeleteVehicleAsync([FromBody] DeleteVehicleManagerRequest request)
    {
        var deleteResponse = _databaseCheckRepository.DeleteVehicleManager(request.ID, request.KvK);

        if (deleteResponse.StatusCode == 200)
        {
            Response.Cookies.Append("LoginVehicleManagerSession", "Invalid cookie", new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                Expires = DateTimeOffset.UtcNow.AddDays(-1)
            });
        }

        return StatusCode(deleteResponse.StatusCode, new { message = deleteResponse.Message });
    }

    [HttpGet("CheckNewEmail")]
    public async Task<IActionResult> CheckNewEmailAsync(string email)
    {
        var emailCheck = await _employeeRepository.checkUsageEmaiVehicleManagerlAsync(email);

        if (!emailCheck.status)
        {
            return StatusCode(200, new { message = "Geldige email" });
        }

        return StatusCode(400, new { message = "Email is al ingebruik" });
    }

    [HttpPut("ChangeVehicleManagerInfo")]
    public async Task<IActionResult> ChangeVehicleManagerInfoAsync([FromBody] ChangeVehicleManagerInfo request)

    {
        if (request == null)
        {
            return BadRequest("Request body cannot be null.");
        }

        Console.WriteLine("ChangeVehicleManagerInfo endpoint hit.");

        try
        {
            // Call the repository method and destructure the result
            (int statusCode, string message) = await _userRepository.ChangeVehicleManagerInfo(request);

            // Return the appropriate status code and message
            return StatusCode(statusCode, new { message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }
}

