using Microsoft.AspNetCore.Mvc;
using WPR.Cryption;
using WPR.Repository;
using WPR.Utils;

namespace WPR.Controllers.Employee.VehicleManager.ChangeBusinessSettings;

[Route("api/[controller]")]
[ApiController]
public class ChangeBusinessSettingsController : ControllerBase
{
    private readonly IUserRepository _userRepository;
    private readonly IEmployeeRepository _employeeRepository;
    private readonly Crypt _crypt;

    public ChangeBusinessSettingsController(IUserRepository userRepository, Crypt crypt, IEmployeeRepository employeeRepository)
    {
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _crypt = crypt ?? throw new ArgumentNullException(nameof(crypt));
        _employeeRepository = employeeRepository ?? throw new ArgumentNullException(nameof(employeeRepository));
    }

    [HttpGet("GetBusinessInfo")]
    public async Task<IActionResult> GetBusinessInfoAsync(int id)
    {
        try
        {
            (int StatusCode, string Message, int KvK) kvkResponse = _employeeRepository.GetKvK(id);

            if (kvkResponse.StatusCode == 200)
            {
                (bool Status, string Message, Dictionary<string, object> Data) businessInfo = _employeeRepository.GetBusinessInfo(kvkResponse.KvK);

                if (businessInfo.Status)
                {
                    (int StatusCode, string Message, Dictionary<string, object> Data) abonnementInfo = _employeeRepository.GetAbonnementType(Convert.ToInt32(businessInfo.Data["Abonnement"]));

                    if (abonnementInfo.StatusCode == 200)
                    {
                        Dictionary<string, object> data = businessInfo.Data;

                        foreach (var element in abonnementInfo.Data)
                        {
                            if (!data.ContainsKey(element.Key))
                            {
                                data[element.Key] = element.Value;
                            }
                        }

                        return StatusCode(200, new { data });
                    }
                    return StatusCode(abonnementInfo.StatusCode, new { message = abonnementInfo.Message });
                }
                return BadRequest(new { message = businessInfo.Message });
            }
            return StatusCode(kvkResponse.StatusCode, kvkResponse.Message);
        }
        catch (OverflowException ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }

    [HttpPut("ChangeBusinessInfo")]
    public async Task<IActionResult> ChangeBusinessInfoAsync([FromBody] ChangeBusinessRequest request)
    {
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

    [HttpGet("CheckNewEmail")]
    public async Task<IActionResult> CheckNewEmailAsync(string email)
    {
        var emailCheck = await _employeeRepository.checkUsageEmaiVehicleManagerlAsync(email);

        if (!emailCheck.status)
        {
            return StatusCode(200, new { message = "Geldige email" });
        }
        return StatusCode(400, new { message = "Email is al ingebruik"} );
    }
}

