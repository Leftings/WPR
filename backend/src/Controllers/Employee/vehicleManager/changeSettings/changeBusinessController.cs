using Microsoft.AspNetCore.Mvc;
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

    // Constructor voor de controller, waar de benodigde repositories worden geïnjecteerd
    public ChangeBusinessSettingsController(IUserRepository userRepository, IEmployeeRepository employeeRepository,
        IDatabaseCheckRepository databaseCheckRepository)
    {
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _employeeRepository = employeeRepository ?? throw new ArgumentNullException(nameof(employeeRepository));
        _databaseCheckRepository =
            databaseCheckRepository ?? throw new ArgumentNullException(nameof(databaseCheckRepository));
    }

    /// <summary>
    /// Verkrijg de bedrijfsinformatie op basis van het ID van de voertuigmanager.
    /// </summary>
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
                // Verkrijg de bedrijfsinformatie op basis van KvK nummer
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
            // Verwerk eventuele fouten tijdens het ophalen van de bedrijfsinformatie
            Console.WriteLine($"Error: {ex.Message}");
            return StatusCode(500, new { message = ex.Message });
        }
    }

    /// <summary>
    /// Wijzig de bedrijfsinformatie. Voert ook een wachtwoordvalidatie uit indien er een nieuw wachtwoord wordt opgegeven.
    /// </summary>
    [HttpPut("ChangeBusinessInfo")]
    public async Task<IActionResult> ChangeBusinessInfoAsync([FromBody] ChangeBusinessRequest request)
    {
        if (request == null)
        {
            return BadRequest("Body cannot be null");
        }

        if (!string.IsNullOrEmpty(request.VehicleManagerInfo.Password))
        {
            // Validatie van het wachtwoord
            (bool Valid, string Message) result = PasswordChecker.IsValidPassword(request.VehicleManagerInfo.Password);

            if (!result.Valid)
            {
                return StatusCode(412, new { message = result.Message });
            }
        }

        // Wijzig de bedrijfsinformatie
        (int StatusCode, string Message) updated = await _userRepository.ChangeBusinessInfo(request);

        return StatusCode(updated.StatusCode, new { message = updated.Message });
    }

    /// <summary>
    /// Verwijder het bedrijfsaccount, inclusief alle gegevens, uit het systeem.
    /// </summary>
    [HttpDelete("DeleteBusiness")]
    public async Task<IActionResult> DeleteUserAsync([FromBody] DeleteBusinessRequest request)
    {
        // Verwijder het bedrijf uit de database
        var deleteResponse = _databaseCheckRepository.DeleteBusiness(request.KvK);

        // Indien succesvol, invalideer de sessiecookie
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

    /// <summary>
    /// Verwijder een voertuigmanager uit het systeem.
    /// </summary>
    [HttpDelete("DeleteVehicleManager")]
    public async Task<IActionResult> DeleteVehicleAsync([FromBody] DeleteVehicleManagerRequest request)
    {
        var deleteResponse = _databaseCheckRepository.DeleteVehicleManager(request.ID, request.KvK);

        // Indien succesvol, invalideer de sessiecookie
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

    /// <summary>
    /// Controleer of een nieuw emailadres al in gebruik is voor de voertuigmanager.
    /// </summary>
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

    /// <summary>
    /// Wijzig de gegevens van de voertuigmanager.
    /// </summary>
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
            // Wijzig de voertuigmanager gegevens
            (int statusCode, string message) = await _userRepository.ChangeVehicleManagerInfo(request);

            return StatusCode(statusCode, new { message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }
}
