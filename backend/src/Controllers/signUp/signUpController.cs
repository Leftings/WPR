using WPR.Utils;

namespace WPR.Controllers.SignUp;

using Microsoft.AspNetCore.Mvc;
using WPR.Database;
using System;
using WPR.Repository;

[Route("api/[controller]")]
[ApiController]
public class SignUpController : ControllerBase
{
    private readonly Connector _connector;
    private readonly IUserRepository _userRepository;

    public SignUpController(Connector connector, IUserRepository userRepository)
    {
        _connector = connector ?? throw new ArgumentNullException(nameof(connector));
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
    }

    private bool IsFilledIn(SignUpRequest signUpRequest)
    {
        return 
        !(signUpRequest == null
        || string.IsNullOrEmpty(signUpRequest.Email)
        || string.IsNullOrEmpty(signUpRequest.Password)
        || string.IsNullOrEmpty(signUpRequest.FirstName)
        || string.IsNullOrEmpty(signUpRequest.LastName)
        || string.IsNullOrEmpty(signUpRequest.TelNumber));
    }

    // Wijzigingen die gemaakt worden in signUpPersonal moeten ook gemaakt worden in signUpEmployee
    [HttpPost("signUpPersonal")]
    public async Task<IActionResult> signUpPersonal([FromBody] SignUpRequest signUpRequest)
    {
        var emailCheck = await _userRepository.checkUsageEmailAsync(signUpRequest.Email);
        bool commit = true;

        using (var connection = _connector.CreateDbConnection())
        using (var transaction = connection.BeginTransaction())
        {
            Console.WriteLine($"Received BirthDate: {signUpRequest.BirthDate}");
            Console.WriteLine($"{signUpRequest.Email} | {signUpRequest.Password} | {signUpRequest.FirstName} | {signUpRequest.LastName} | {signUpRequest.TelNumber} {signUpRequest.Adres} | {signUpRequest.BirthDate}");
            try
            {
                bool filledIn = IsFilledIn(signUpRequest);
                if (!filledIn)
                {
                    return BadRequest(new { message = "Not all elements are filled in" });
                }
                
                if (!EmailChecker.IsValidEmail(signUpRequest.Email))
                {
                    return BadRequest(new { message = "Invalid email format" });
                }

                if (!BirthdayChecker.IsValidBirthday(signUpRequest.BirthDate))
                {
                    return BadRequest(new { message = "Invalid birthday format" });
                }

                if (!TelChecker.IsValidPhoneNumber(signUpRequest.TelNumber))
                {
                    return BadRequest(new { message = "Invalid phone number" });
                }
                
                else if (emailCheck.status)
                {
                    transaction.Rollback();
                    commit = false;

                    return BadRequest( new { message = "Email already exists"} );
                }
                else if (filledIn && signUpRequest.BirthDate != null)
                {
                    var customer = await _userRepository.addCustomerAsync(new object[] 
                    {
                        signUpRequest.Adres,
                        signUpRequest.TelNumber,
                        signUpRequest.Password,
                        signUpRequest.Email,
                        signUpRequest.FirstName,
                        signUpRequest.LastName
                    });

                    var personal = await _userRepository.addPersonalCustomerAsync(new object[] 
                    {
                        customer.newUserID, 
                        signUpRequest.BirthDate
                    });

                    if (!personal.status)
                    {
                        transaction.Rollback();
                        commit = false;

                        return BadRequest(new { personal.message });
                    }
                    return Ok(new { personal.message });
                }
                else
                {
                    return BadRequest(new { message = "Something went wrong (Nothing matched)" });
                }
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                commit = false;

                return StatusCode(500, ex);
            }
            finally
            {
                if (commit)
                {
                    transaction.Commit();
                }

                connection.Close();
            }
        }
    }

    [HttpPost("signUpEmployee")]
    public async Task<IActionResult> signUpEmployee([FromBody] SignUpRequest signUpRequest)
    {
        var connection = _connector.CreateDbConnection();
        var emailCheck = await _userRepository.checkUsageEmailAsync(signUpRequest.Email);
        bool commit = true;

        using (var transaction = connection.BeginTransaction())
        {
            try
            {
                bool filledIn = IsFilledIn(signUpRequest);
                if (!filledIn )
                {
                    return BadRequest(new { message = "Not all elements are filled in" });
                }
                if (!EmailChecker.IsValidEmail(signUpRequest.Email))
                {
                    return BadRequest(new { message = "Invalid email format" });
                }

                var kvkChecker = new KvkChecker(_userRepository);
                var (isValidKvk, kvkErrorMessage) = await kvkChecker.IsKvkNumberValid(signUpRequest.KvK);

                if (!isValidKvk)
                {
                    return BadRequest(new { message = kvkErrorMessage });
                }

                if (!TelChecker.IsValidPhoneNumber(signUpRequest.TelNumber))
                {
                    return BadRequest(new { message = "Invalid phone number" });
                }
                else if (emailCheck.status)
                {
                    transaction.Rollback();
                    commit = false;

                    return BadRequest( new { message = "Email already exists"} );
                }
                else if (filledIn && signUpRequest.KvK != null)
                {
                    var customer = await _userRepository.addCustomerAsync(new object[] 
                    {
                        signUpRequest.Adres,
                        signUpRequest.TelNumber,
                        signUpRequest.Password,
                        signUpRequest.Email,
                        signUpRequest.FirstName,
                        signUpRequest.LastName
                    });
                    var employee = await _userRepository.addEmployeeCustomerAsync(new object[] 
                    {
                        customer.newUserID,
                        signUpRequest.KvK
                    });

                    if (!employee.status)
                    {
                        transaction.Rollback();
                        commit = false;

                        return BadRequest(new { employee.status });
                    }


                    return Ok(new { employee.status });
                }
                else
                {
                    return BadRequest(new { message = "Something went wrong (Nothing mathced)" });
                }
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                commit = false;
                
                return StatusCode(500, ex);
            }
            finally
            {
                if (commit)
                {
                    transaction.Commit();
                }

                connection.Close();
            }
        }
        
    }
}