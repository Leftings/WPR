using WPR.Services;
using WPR.Utils;

namespace WPR.Controllers.General.SignUp;

using Microsoft.AspNetCore.Mvc;
using WPR.Database;
using System;
using WPR.Repository;
using WPR.Hashing;
using WPR.Data;
using Microsoft.VisualBasic;

/// <summary>
/// SignUpController zorgt ervoor dat persoonlijke en zakelijke accounts aangemaakt kunnen worden
/// </summary>
[Route("api/[controller]")]
[ApiController]
public class SignUpController : ControllerBase
{
    private readonly IUserRepository _userRepository;

    public SignUpController(IUserRepository userRepository)
    {
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
    }
    

    [HttpPost("signUp")]
    public async Task<IActionResult> SignUp([FromForm] CombinedSignUpRequest signUpRequest)
    {
        SignUpRequestCustomer signUpRequestCustomer = signUpRequest.SignUpRequestCustomer;
        SignUpRequestCustomerPrivate signUpRequestCustomerPrivate = signUpRequest.SignUpRequestCustomerPrivate;

        (int StatusCode, string Message) response = await _userRepository.AddCustomer(signUpRequestCustomer, signUpRequestCustomerPrivate);

        return StatusCode(response.StatusCode, new { message = response.Message });
    }

    [HttpPost("signUpPersonal")]
    public async Task<IActionResult> signUpPersonalAsync([FromForm] SignUpRequest signUpRequest)
    {
        Console.WriteLine(signUpRequest);
        (bool Status, string Message) response = await _userRepository.AddPersonalCustomer(signUpRequest);

        if (response.Status)
        {
            return Ok( new { message = response.Message });
        }
        return BadRequest( new { message = response.Message });
    }
}