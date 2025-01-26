using WPR.Services;

namespace WPR.Controllers.General.SignUp;

using Microsoft.AspNetCore.Mvc;
using WPR.Database;
using System;
using WPR.Repository;
using WPR.Hashing;
using WPR.Data;

/// <summary>
/// SignUpController zorgt ervoor dat persoonlijke en zakelijke accounts aangemaakt kunnen worden.
/// </summary>
[Route("api/[controller]")]
[ApiController]
public class SignUpController : ControllerBase
{
    // Afhankelijkheden die via dependency injection worden geïnjecteerd
    private readonly Connector _connector;
    private readonly IUserRepository _userRepository;
    private readonly EnvConfig _envConfig;
    private readonly Hash _hash;
    private readonly EmailService _emailService;

    /// <summary>
    /// Constructor voor de controller die alle noodzakelijke afhankelijkheden injecteert.
    /// </summary>
    /// <param name="connector">Connector voor databaseverbindingen of andere verbindingen</param>
    /// <param name="userRepository">Repository voor gebruikersgerelateerde database-operaties</param>
    /// <param name="envConfig">Configuratieobject voor omgevingsinstellingen</param>
    /// <param name="hash">Klasse voor het hashen van wachtwoorden</param>
    /// <param name="emailService">Service voor het versturen van e-mails</param>
    public SignUpController(Connector connector, IUserRepository userRepository, EnvConfig envConfig, Hash hash,
        EmailService emailService)
    {
        _connector = connector ?? throw new ArgumentNullException(nameof(connector));
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _envConfig = envConfig ?? throw new ArgumentNullException(nameof(envConfig));
        _hash = hash ?? throw new ArgumentNullException(nameof(hash));
        _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
    }

    /// <summary>
    /// Controleert of alle noodzakelijke velden in het aanmeldingsformulier zijn ingevuld.
    /// </summary>
    /// <param name="signUpRequest">De aanmeldingsgegevens van de gebruiker</param>
    /// <returns>True als alle vereiste velden zijn ingevuld, anders False</returns>
    private bool IsFilledIn(SignUpRequest signUpRequest)
    {
        return
            !(signUpRequest == null
              || string.IsNullOrEmpty(signUpRequest.Email) // Controleert of e-mail is ingevuld
              || string.IsNullOrEmpty(signUpRequest.Password) // Controleert of wachtwoord is ingevuld
              || string.IsNullOrEmpty(signUpRequest.FirstName) // Controleert of voornaam is ingevuld
              || string.IsNullOrEmpty(signUpRequest.LastName) // Controleert of achternaam is ingevuld
              || string.IsNullOrEmpty(signUpRequest.TelNumber)); // Controleert of telefoonnummer is ingevuld
    }

    /// <summary>
    /// Endpoint voor het registreren van een zakelijke klant met alle benodigde gegevens.
    /// </summary>
    /// <param name="signUpRequest">Gecombineerde aanvraag voor zakelijke klantregistratie</param>
    /// <returns>Een HTTP-statuscode met een bericht over het resultaat van de registratie</returns>
    [HttpPost("signUp")]
    public async Task<IActionResult> SignUp([FromForm] CombinedSignUpRequest signUpRequest)
    {
        // Haal klant- en privégegevens uit de gecombineerde aanvraag
        SignUpRequestCustomer signUpRequestCustomer = signUpRequest.SignUpRequestCustomer;
        SignUpRequestCustomerPrivate signUpRequestCustomerPrivate = signUpRequest.SignUpRequestCustomerPrivate;

        // Probeer de klant toe te voegen via de repository
        (int StatusCode, string Message) response =
            await _userRepository.AddCustomer(signUpRequestCustomer, signUpRequestCustomerPrivate);

        // Retourneer een statuscode met het resultaatbericht van de toevoeging
        return StatusCode(response.StatusCode, new { message = response.Message });
    }

    /// <summary>
    /// Endpoint voor het registreren van een persoonlijke klant met alleen persoonlijke gegevens.
    /// </summary>
    /// <param name="signUpRequest">De aanmeldingsgegevens van de persoonlijke klant</param>
    /// <returns>Een HTTP-statuscode met een bericht over het resultaat van de registratie</returns>
    [HttpPost("signUpPersonal")]
    public async Task<IActionResult> signUpPersonalAsync([FromForm] SignUpRequest signUpRequest)
    {
        // Log de gegevens van de aanvraag voor debug-doeleinden
        Console.WriteLine(signUpRequest);

        // Probeer de persoonlijke klant toe te voegen via de repository
        (bool Status, string Message) response = await _userRepository.AddPersonalCustomer(signUpRequest);

        // Als de klant succesvol is toegevoegd, stuur dan een OK-antwoord
        if (response.Status)
        {
            return Ok(new { message = response.Message });
        }

        // Als er een fout is, stuur dan een BadRequest-antwoord met het bericht
        return BadRequest(new { message = response.Message });
    }
}
