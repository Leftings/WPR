using Employee.Repository;
using Employee.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualBasic;

namespace Employee.Controllers.signUpStaff;

[Route("api/[controller]")]
[ApiController]
public class SignUpStaffController : ControllerBase
{
    private readonly IUserRepository _userRepository;

    public SignUpStaffController(IUserRepository userRepository)
    {
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
    }

    [HttpPost("signUpStaff")]
    public async Task<IActionResult> SignUpStaff([FromBody] SignUpRequest signUpRequest)
    {
        Object[] personData = new Object[] {signUpRequest.FirstName, signUpRequest.LastName, signUpRequest.Password, signUpRequest.Email, signUpRequest.Office};
        var emailCheckTask = _userRepository.checkUsageEmailAsync(signUpRequest.Email);
        var addStaffTask = _userRepository.AddStaff(personData);

        if (!EmailChecker.IsValidEmail(signUpRequest.Email))
        {
            return BadRequest(new { message = "Invalid email format" });
        }

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

        var addStaff = await addStaffTask;
        if (addStaff.status)
        {
            return Ok( new { message = "Data inserted"});
        }
        return BadRequest (new { messsage = addStaff.message });
    }
}