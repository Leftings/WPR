using WPR.Repository;
using WPR.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualBasic;

namespace WPR.Controllers.signUpStaff;

/// <summary>
/// SignUpStaffController zorgt ervoor dat er front-, backoffice en wagenparkbeheerders toegevoegd kunnen worden aan het systeem
/// </summary>
[Route("api/[controller]")]
[ApiController]
public class SignUpStaffController : ControllerBase
{
    private readonly IEmployeeRepository _userRepository;

    public SignUpStaffController(IEmployeeRepository userRepository)
    {
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
    }

    /// <summary>
    /// Car and All medewerkers / Vehicle Managers kunnen hier aangemaakt worden.
    /// Mocht het emailadres al bestaan of ongelig zijn, dan wordt de aanvraag geweigerd.
    /// Mocht het wachtwoord niet aan de eisen (minimaal 10 karakters, 1 hoofdletter, 1 kleine letter, 1 cijfer en 1 symbool), dan wordt de aanvraag geweigerd.
    /// </summary>
    /// <param name="signUpRequest"></param>
    /// <returns></returns>
    [HttpPost("signUpStaff")]
    public async Task<IActionResult> SignUpStaff([FromBody] SignUpRequest signUpRequest)
    {
        Object[] personData = new Object[] {signUpRequest.FirstName, signUpRequest.LastName, signUpRequest.Password, signUpRequest.Email, signUpRequest.Job, signUpRequest.KvK};
        
        // Check of het emailadres al in gebruik is
        var emailCheckTask = _userRepository.checkUsageEmailAsync(signUpRequest.Email);
        //var addStaffTask = _userRepository.AddStaff(personData);

        // Check of de email een geldige email is
        if (!EmailChecker.IsValidEmail(signUpRequest.Email))
        {
            return BadRequest(new { message = "Invalid email format" });
        }

        // Check is het wachtwoord bestaat uit 10 karakters, 1 hoofletter, 1 kleine letter, 1 cijfer en 1 speciaal karakter
        (bool status,string message) statusPassword = PasswordChecker.IsValidPassword(signUpRequest.Password);
        if (!statusPassword.status)
        {
            return BadRequest( new { message = statusPassword.message });
        }

        var emailCheck = await emailCheckTask;
        if (emailCheck.status)
        {
            return BadRequest(new { message = "Email allready in use"});
        }

        //var addStaff = await addStaffTask;
        var addStaff = await _userRepository.AddStaff(personData);
        if (addStaff.status)
        {
            return Ok( new { message = "Data inserted"});
        }

        if (addStaff.message.Equals("Cannot add or update a child row: a foreign key constraint fails (`WPR`.`VehicleManager`, CONSTRAINT `VehicleManager_ibfk_1` FOREIGN KEY (`Business`) REFERENCES `Business` (`KVK`))"))
        {
            return BadRequest(new { message = "KvK nummer is niet geregistreerd"});
        }

        return BadRequest(new { messsage = addStaff.message });
    }
}