using WPR.Utils;

namespace WPR.SignUp;

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
        var connection = _connector.CreateDbConnection();
        var emailCheck = await _userRepository.checkUsageEmailAsync(connection, signUpRequest.Email);
        bool commit = true;

        using (var transaction = connection.BeginTransaction())
        {
            Console.WriteLine($"Received BirthDate: {signUpRequest.BirthDate}");
            Console.WriteLine($"{signUpRequest.Email} | {signUpRequest.Password} | {signUpRequest.FirstName} | {signUpRequest.LastName} | {signUpRequest.TelNumber} {signUpRequest.Adres} | {signUpRequest.BirthDate}");
            try
            {
                bool filledIn = IsFilledIn(signUpRequest);
                if (!filledIn || signUpRequest.BirthDate == null)
                {
                    return BadRequest(new { message = "Not all elements are filled in" });
                }
                
                if (!EmailChecker.IsValidEmail(signUpRequest.Email))
                {
                    return BadRequest(new { message = "Invalid email format" });
                }

                if (!TelChecker.IsValidPhoneNumber(signUpRequest.TelNumber))
                {
                    return BadRequest(new { message = "Invalid phone number" });
                }
                
                else if (emailCheck.status)
                {
                    transaction.Rollback();
                    commit = false;

                    return BadRequest( new { message = "Email allready existing"} );
                }
                else if (filledIn && signUpRequest.BirthDate != null)
                {
                    var customer = await _userRepository.addCustomerAsync(connection, new object[] 
                    {
                        signUpRequest.Adres,
                        signUpRequest.TelNumber,
                        signUpRequest.Password,
                        signUpRequest.Email
                    });

                    var personal = await _userRepository.addPersonalCustomerAsync(connection, new object[] 
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
        var emailCheck = await _userRepository.checkUsageEmailAsync(connection, signUpRequest.Email);
        bool commit = true;

        using (var transaction = connection.BeginTransaction())
        {
            try
            {
                bool filledIn = IsFilledIn(signUpRequest);
                if (!filledIn || signUpRequest.KvK == null)
                {
                    return BadRequest(new { message = "Not all elements are filled in" });
                }
                else if (emailCheck.status)
                {
                    transaction.Rollback();
                    commit = false;

                    return BadRequest( new { message = "Email allready existing"} );
                }
                else if (filledIn && signUpRequest.KvK != null)
                {
                    var customer = await _userRepository.addCustomerAsync(connection, new object[] 
                    {
                        signUpRequest.Adres,
                        signUpRequest.TelNumber,
                        signUpRequest.Password,
                        signUpRequest.Email
                    });
                    var employee = await _userRepository.addEmployeeCustomerAsync(connection,new object[] 
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

public class SignUpRequest
{
    public string? Email { get; set; }
    public string? Password { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string TelNumber { get; set; }
    public string? Adres { get; set; }
    public DateTime? BirthDate { get; set; }
    public int? KvK { get; set; }
}