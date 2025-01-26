namespace WPR.Controllers.Customer.AddBusiness;

using Microsoft.AspNetCore.Mvc;
using WPR.Controllers.Employee.BackOffice.signUpStaff;
using WPR.Repository;
using WPR.Services;
using WPR.Utils;

/// <summary>
/// Controller voor het verbinden van het aanmaken van bedrijven op basis van het KVK nummer.
/// Deze controller behandelt de aanvragen voor nieuwe bedrijven, de goedkeuring of afwijzing van bedrijven, en het versturen van e-mails.
/// </summary>
[Route("api/[controller]")]
[ApiController]
public class AddBusinessController : ControllerBase
{
    private readonly IEmployeeRepository _employeeRepository;
    private readonly IEmailService _emailService;

    public AddBusinessController (IEmployeeRepository employeeRepository, IEmailService emailService)
    {
        _employeeRepository = employeeRepository ?? throw new ArgumentNullException(nameof(employeeRepository));
        _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
    }

    /// <summary>
    /// Voegt een nieuw bedrijf toe op basis van het KVK-nummer en stuurt een response terug.
    /// </summary>
    /// <param name="request">Het verzoek om een nieuw bedrijf toe te voegen.</param>
    /// <returns>Retourneert de status en een boodschap over het resultaat van de toevoeging.</returns>
    [HttpPost("addBusiness")]
    public async Task<IActionResult> AddBusiness([FromForm] AddBusinessRequest request)
    {
        var output = await _employeeRepository.AddBusiness(request);
        
        if (output.status)
        {
            return Ok(new addBusinessResponse{ Message = output.message });
        }
        return BadRequest(new addBusinessResponse { Message = output.message });
    }

    /// <summary>
    /// Haalt een lijst op van nieuwe bedrijfsaanvragen die nog niet zijn goedgekeurd.
    /// </summary>
    /// <returns>Retourneert een lijst van KVK-nummers van nieuwe aanvragen.</returns>
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

    /// <summary>
    /// Haalt gedetailleerde informatie op over een specifiek bedrijf op basis van het KVK-nummer.
    /// </summary>
    /// <param name="kvk">Het KVK-nummer van het bedrijf.</param>
    /// <returns>Retourneert de gedetailleerde informatie over het bedrijf.</returns>
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

    /// <summary>
    /// Markeert een bedrijfsaanvraag als geaccepteerd, genereert een gebruikerswachtwoord voor de beheerder en stuurt een bevestigingsmail.
    /// </summary>
    /// <param name="kvk">Het KVK-nummer van het bedrijf.</param>
    /// <returns>Retourneert een statusbericht over de goedkeuring en e-mailverzending.</returns>
    [HttpPut("businessAccepted")]
    public async Task<IActionResult> BusinessAccepted(int kvk)
    {
        string emailSend = "No Email Send";
        (int StatusCode, string Message) response = _employeeRepository.BusinessAccepted(kvk);

        if (response.StatusCode == 200)
        {
            (bool Status, string Message, Dictionary<string, object> Data) info = _employeeRepository.GetBusinessInfo(kvk);

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

    /// <summary>
    /// Markeert een bedrijfsaanvraag als afgewezen en stuurt een afwijzingsmail.
    /// </summary>
    /// <param name="kvk">Het KVK-nummer van het bedrijf.</param>
    /// <returns>Retourneert een statusbericht over de afwijzing en e-mailverzending.</returns>
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
