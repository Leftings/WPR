namespace WPR.Controllers.AddBusiness;

using System.ComponentModel;
using Microsoft.AspNetCore.Mvc;
using WPR.Controllers.signUpStaff;
using WPR.Repository;
using WPR.Services;
using WPR.Utils;

/// <summary>
/// Controller voor het verbinden van het aanmaken van bedrijven op basis van het KVK nummer
/// </summary>
[Route("api/[controller]")]
[ApiController]
public class AddBusinessController : ControllerBase
{
    private readonly IEmployeeRepository _employeeRepository;
    private readonly EmailService _emailService;

    public AddBusinessController (IEmployeeRepository employeeRepository, EmailService emailService)
    {
        _employeeRepository = employeeRepository ?? throw new ArgumentNullException(nameof(employeeRepository));
        _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
    }

    /// <summary>
    /// Maakt een connectie met UserRepository en geeft het antwoord terug
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost("addBusiness")]
    public async Task<IActionResult> AddBusiness([FromForm] AddBusinessRequest request)
    {
        var output = await _employeeRepository.AddBusiness(request);
        
        if (output.status)
        {
            return Ok(new { output.message });
        }
        return BadRequest(new { output.message });
    }

    [HttpGet("getNewBusinesses")]
    public async Task<IActionResult> GetNewBusinesses()
    {
        (int StatusCode, string Message, IList<int> KvK) response = _employeeRepository.ViewBusinessRequests();
        
        if (response.StatusCode == 200)
        {
            return StatusCode(response.StatusCode, new { message = response.KvK });
        }
        return StatusCode(response.StatusCode, new { message = response.Message });
    }

    [HttpGet("getNewBusiness")]
    public async Task<IActionResult> GetNewBusiness(int kvk)
    {
        (int StatusCode, string Message, Dictionary<string, object> Data) response = await _employeeRepository.ViewBusinessRequestDetailed(kvk);

        if (response.StatusCode == 200)
        {
            return StatusCode(response.StatusCode, new { message = response.Message, data = response.Data });
        }
        return StatusCode(response.StatusCode, new { message = response.Message });
    }

    [HttpPut("businessAccepted")]
    public async Task<IActionResult> BusinessAccepted(int kvk)
    {
        string emailSend = "No Email Send";
        (int StatusCode, string Message) response = _employeeRepository.BusinessAccepted(kvk);

        if (response.StatusCode == 200)
        {
            (bool Status, string Message, Dictionary<string, object> Data) info = _employeeRepository.GetBusinessInfo(kvk);
            Console.WriteLine(info.Message);

            if (info.Status)
            {
                string password = StrongPasswordMaker.CreatePassword();
                await _employeeRepository.AddStaff(new SignUpStaffRequest()
                    {FirstName = "Vehicle", LastName = "Manager", Password = password, Job = "Wagen", KvK = Convert.ToInt32(info.Data["KvK"]), Email = $"wagenparkbeheerder{info.Data["Domain"]}"
                });
                _emailService.SendBusinessReviewEmail((string)info.Data["ContactEmail"], (string)info.Data["BusinessName"], (string)info.Data["Domain"], password, true);
                emailSend = "Email Send";
            }
        }

        return StatusCode(response.StatusCode, new { message = $"{response.Message}\n{emailSend}" });
    }

    [HttpDelete("businessDenied")]
    public async Task<IActionResult> BusinessDenied(int kvk)
    {
        string emailSend = "No Email Send";
        (bool Status, string Message, Dictionary<string, object> Data) info = _employeeRepository.GetBusinessInfo(kvk);
        (int StatusCode, string Message) response = _employeeRepository.BusinessDenied(kvk);


        if (response.StatusCode == 200)
        {
            if (info.Status)
            {
                _emailService.SendBusinessReviewEmail((string)info.Data["ContactEmail"], (string)info.Data["BusinessName"], null, null, false);
                emailSend = "Email Send";
            }
        }
        
        return StatusCode(response.StatusCode, new { message = $"{response.Message}\n{emailSend}" });
    }
}