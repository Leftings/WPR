namespace WPR.Controllers.General.SignUp;

using Microsoft.AspNetCore.Mvc;
using System;
using WPR.Repository;

/// <summary>
/// De SignUpController beheert het aanmaken van zowel persoonlijke als zakelijke accounts.
/// Deze controller ontvangt verzoeken voor registratie en verwerkt ze via de repository.
/// </summary>
[Route("api/[controller]")]
[ApiController]
public class SignUpController : ControllerBase
{
    private readonly IUserRepository _userRepository;

    /// <summary>
    /// Constructor voor de SignUpController. Deze maakt gebruik van de UserRepository voor accountbeheer.
    /// </summary>
    /// <param name="userRepository">De repository die verantwoordelijk is voor het beheren van gebruikersgegevens.</param>
    public SignUpController(IUserRepository userRepository)
    {
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
    }
    
    /// <summary>
    /// Registreert een nieuwe gebruiker met zowel persoonlijke als zakelijke gegevens.
    /// </summary>
    /// <param name="signUpRequest">De gecombineerde registratiegegevens voor de klant en diens bedrijf (indien van toepassing).</param>
    /// <returns>Een HTTP-resultaat met een statuscode en een bericht over het succes of falen van de registratie.</returns>
    [HttpPost("signUp")]
    public async Task<IActionResult> SignUp([FromForm] CombinedSignUpRequest signUpRequest)
    {
        SignUpRequestCustomer signUpRequestCustomer = signUpRequest.SignUpRequestCustomer;
        SignUpRequestCustomerPrivate signUpRequestCustomerPrivate = signUpRequest.SignUpRequestCustomerPrivate;

        (int StatusCode, string Message) response = await _userRepository.AddCustomer(signUpRequestCustomer, signUpRequestCustomerPrivate);

        return StatusCode(response.StatusCode, new { message = response.Message });
    }

    /// <summary>
    /// Registreert een persoonlijke klant zonder zakelijke gegevens.
    /// </summary>
    /// <param name="signUpRequest">De registratiegegevens voor een persoonlijke klant.</param>
    /// <returns>Een HTTP-resultaat met een statuscode en een bericht over het succes of falen van de registratie.</returns>
    [HttpPost("signUpPersonal")]
    public async Task<IActionResult> signUpPersonalAsync([FromForm] SignUpRequest signUpRequest)
    {
        Console.WriteLine(signUpRequest);
        (bool Status, string Message) response = await _userRepository.AddPersonalCustomer(signUpRequest);

        if (response.Status)
        {
            return Ok(new { message = response.Message });
        }
        return BadRequest(new { message = response.Message });
    }
}
