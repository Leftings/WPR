namespace WPR.Controllers.Customer.AddBusiness;

using Microsoft.AspNetCore.Mvc;
using WPR.Controllers.Employee.BackOffice.signUpStaff;
using WPR.Repository;
using WPR.Services;
using WPR.Utils;

/// <summary>
/// Controller voor het verbinden van het aanmaken van bedrijven op basis van het KVK nummer
/// Deze controller beheert verzoeken voor het toevoegen, accepteren en weigeren van bedrijven.
/// </summary>
[Route("api/[controller]")]
[ApiController]
public class AddBusinessController : ControllerBase
{
    private readonly IEmployeeRepository _employeeRepository;
    private readonly EmailService _emailService;

    // Constructor die de benodigde services injecteert.
    public AddBusinessController (IEmployeeRepository employeeRepository, EmailService emailService)
    {
        _employeeRepository = employeeRepository ?? throw new ArgumentNullException(nameof(employeeRepository));
        _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
    }

    /// <summary>
    /// Maakt een bedrijf aan door een KVK-nummer te verwerken en het bedrijf toe te voegen aan de database.
    /// </summary>
    /// <param name="request">De gegevens van het bedrijf die via een formulier zijn verzonden.</param>
    /// <returns>Een HTTP-resultaat met een boodschap over het resultaat van de aanvraag.</returns>
    [HttpPost("addBusiness")]
    public async Task<IActionResult> AddBusiness([FromForm] AddBusinessRequest request)
    {
        var output = await _employeeRepository.AddBusiness(request);
        
        // Als het toevoegen succesvol was, stuur dan een succesbericht terug.
        if (output.status)
        {
            return Ok(new { output.message });
        }
        // Als het toevoegen mislukte, stuur dan een foutmelding terug.
        return BadRequest(new { output.message });
    }

    /// <summary>
    /// Haalt een lijst op van nieuwe bedrijven die zich hebben aangemeld, op basis van hun KvK-nummers.
    /// </summary>
    /// <returns>Een lijst van KvK-nummers van de nieuwe bedrijven.</returns>
    [HttpGet("getNewBusinesses")]
    public async Task<IActionResult> GetNewBusinesses()
    {
        (int StatusCode, string Message, IList<int> KvK) response = _employeeRepository.ViewBusinessRequests();
        
        // Als de verzoek succesvol is, geef de KvK-nummers van de nieuwe bedrijven terug.
        if (response.StatusCode == 200)
        {
            return StatusCode(response.StatusCode, new { message = response.KvK });
        }
        // Anders geef je een foutmelding terug.
        return StatusCode(response.StatusCode, new { message = response.Message });
    }

    /// <summary>
    /// Haalt gedetailleerde informatie op van een specifiek bedrijf, op basis van het KvK-nummer.
    /// </summary>
    /// <param name="kvk">Het KvK-nummer van het bedrijf.</param>
    /// <returns>Gedetailleerde gegevens over het bedrijf.</returns>
    [HttpGet("getNewBusiness")]
    public async Task<IActionResult> GetNewBusiness(int kvk)
    {
        (int StatusCode, string Message, Dictionary<string, object> Data) response = await _employeeRepository.ViewBusinessRequestDetailed(kvk);

        // Als het ophalen van gegevens succesvol was, stuur de gegevens terug.
        if (response.StatusCode == 200)
        {
            return StatusCode(response.StatusCode, new { message = response.Message, data = response.Data });
        }
        // Anders geef je een foutmelding terug.
        return StatusCode(response.StatusCode, new { message = response.Message });
    }

    /// <summary>
    /// Markeer een bedrijf als geaccepteerd en stuur een welkomse-mail naar het bedrijf.
    /// </summary>
    /// <param name="kvk">Het KvK-nummer van het bedrijf.</param>
    /// <returns>Het resultaat van het accepteren van het bedrijf en het versturen van de e-mail.</returns>
    [HttpPut("businessAccepted")]
    public async Task<IActionResult> BusinessAccepted(int kvk)
    {
        string emailSend = "No Email Send";
        (int StatusCode, string Message) response = _employeeRepository.BusinessAccepted(kvk);

        // Als het bedrijf succesvol is geaccepteerd
        if (response.StatusCode == 200)
        {
            (bool Status, string Message, Dictionary<string, object> Data) info = _employeeRepository.GetBusinessInfo(kvk);

            // Als de bedrijfsinformatie succesvol is opgehaald
            if (info.Status)
            {
                // Genereer een sterk wachtwoord voor de nieuwe werknemer
                string password = StrongPasswordMaker.CreatePassword();
                
                // Voeg een nieuwe werknemer toe aan het systeem (wagenparkbeheerder)
                await _employeeRepository.AddStaff(new SignUpStaffRequest()
                    {
                        FirstName = "Vehicle", 
                        LastName = "Manager", 
                        Password = password, 
                        Job = "Wagen", 
                        KvK = Convert.ToInt32(info.Data["KvK"]), 
                        Email = $"wagenparkbeheerder{info.Data["Domain"]}"
                    });
                
                // Stuur een welkomse-mail naar het bedrijf
                _emailService.SendBusinessReviewEmail((string)info.Data["ContactEmail"], (string)info.Data["BusinessName"], (string)info.Data["Domain"], password, true);
                emailSend = "Email Send";
            }
        }

        // Retourneer het resultaat van het proces
        return StatusCode(response.StatusCode, new { message = $"{response.Message}\n{emailSend}" });
    }

    /// <summary>
    /// Markeer een bedrijf als geweigerd en stuur een afwijzingsmail naar het bedrijf.
    /// </summary>
    /// <param name="kvk">Het KvK-nummer van het bedrijf.</param>
    /// <returns>Het resultaat van het weigeren van het bedrijf en het versturen van de e-mail.</returns>
    [HttpDelete("businessDenied")]
    public async Task<IActionResult> BusinessDenied(int kvk)
    {
        string emailSend = "No Email Send";
        (bool Status, string Message, Dictionary<string, object> Data) info = _employeeRepository.GetBusinessInfo(kvk);
        (int StatusCode, string Message) response = _employeeRepository.BusinessDenied(kvk);

        // Als het bedrijf succesvol is geweigerd
        if (response.StatusCode == 200)
        {
            // Als de bedrijfsinformatie beschikbaar is
            if (info.Status)
            {
                // Stuur een afwijzingsmail naar het bedrijf
                _emailService.SendBusinessReviewEmail((string)info.Data["ContactEmail"], (string)info.Data["BusinessName"], null, null, false);
                emailSend = "Email Send";
            }
        }
        
        // Retourneer het resultaat van het proces
        return StatusCode(response.StatusCode, new { message = $"{response.Message}\n{emailSend}" });
    }
}
