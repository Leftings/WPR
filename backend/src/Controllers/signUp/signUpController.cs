using WPR.Services;
using WPR.Utils;

namespace WPR.Controllers.SignUp;

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
    private readonly Connector _connector;
    private readonly IUserRepository _userRepository;
    private readonly EnvConfig _envConfig;
    private readonly Hash _hash;
    private readonly EmailService _emailService;

    public SignUpController(Connector connector, IUserRepository userRepository, EnvConfig envConfig, Hash hash, EmailService emailService)
    {
        _connector = connector ?? throw new ArgumentNullException(nameof(connector));
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _envConfig = envConfig ?? throw new ArgumentNullException(nameof(envConfig));
        _hash = hash ?? throw new ArgumentNullException(nameof(hash));
        _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
    }

    /// <summary>
    /// Er wordt gekeken of alle noodzakelijke velden voor het aanmaken van een account ingevuld zijn
    /// </summary>
    /// <param name="signUpRequest"></param>
    /// <returns></returns>
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
    
    /// <summary>
    /// Een account voor particuliere gebruikers kan aangemaakt worden (onderscheiden door geboortedatum)
    /// </summary>
    /// <param name="signUpRequest"></param>
    /// <returns></returns>
    // Wijzigingen die gemaakt worden in signUpPersonal moeten ook gemaakt worden in signUpEmployee
    /*[HttpPost("signUpPersonal")]
    public async Task<IActionResult> signUpPersonalAsync([FromBody] SignUpRequest signUpRequest)
    {
        // Het emailadres wordt gecontrolleerd of het emailadres al bestaat
        var emailCheck =  _userRepository.checkUsageEmailAsync(signUpRequest.Email);
        bool commit = true;

        using (var connection = _connector.CreateDbConnection())
        using (var transaction = connection.BeginTransaction())
        {
            Console.WriteLine("X");
            Console.WriteLine($"Received BirthDate: {signUpRequest.BirthDate}");
            Console.WriteLine($"{signUpRequest.Email} | {signUpRequest.Password} | {signUpRequest.FirstName} | {signUpRequest.LastName} | {signUpRequest.TelNumber} {signUpRequest.Adres} | {signUpRequest.BirthDate}");
            try
            {
                // Er wordt gekeken of alle gegevens ingevuld zijn
                bool filledIn = IsFilledIn(signUpRequest);
                if (!filledIn)
                {
                    return BadRequest(new { message = "Not all elements are filled in" });
                }
                
                // Er wordt gekeken of het opgegeven emailadres valid is
                if (!EmailChecker.IsValidEmail(signUpRequest.Email))
                {
                    return BadRequest(new { message = "Invalid email format" });
                }

                // Er wordt gekeken of er een geldige geboorte datum ingevuld is (niet groter dan 120)
                if (!BirthdayChecker.IsValidBirthday(signUpRequest.BirthDate))
                {
                    return BadRequest(new { message = "Invalid birthday format" });
                }

                // Er wordt gekeken of het ingevoerde telefoonnummer een geldig nederlands nummer is
                if (!TelChecker.IsValidPhoneNumber(signUpRequest.TelNumber))
                {
                    return BadRequest(new { message = "Invalid phone number" });
                }
                
                // Er wordt gekeken of het wachtwoord bestaat uit minstens 10 karakters, 1 hoofdletter, 1 kleine letter, 1 cijfer en 1 speciaal teken
                var (isPasswordValid, passwordError) = PasswordChecker.IsValidPassword(signUpRequest.Password);
                var emailStatus = await emailCheck;

                if (!isPasswordValid)
                {
                    return BadRequest(new { message = passwordError });
                }

                // Als het emailadres al bestaat worden de gegevens gerollbacked
                else if (emailStatus.status)
                {
                    transaction.Rollback();
                    commit = false;

                    return BadRequest( new { message = "Email already exists"} );
                }

                // Als alle velden zijn ingevuld en de geboorte datum is ingevuld, wordt er een nieuwe customer aangemaakt
                else if (filledIn && signUpRequest.BirthDate != null)
                {
                    // Er wordt een nieuwe gebruiker aangemaakt in de tabel UserCustomer
                    var customer = await _userRepository.addCustomerAsync(new object[] 
                    {
                        signUpRequest.Adres,
                        signUpRequest.TelNumber,
                        _hash.createHash(signUpRequest.Password),
                        signUpRequest.Email,
                        signUpRequest.FirstName,
                        signUpRequest.LastName
                    });

                    // Er wordt aan de nieuwe customer het opgegeven geboortedatum gekoppeld in de tabel UserPersonal
                    var personal = await _userRepository.addPersonalCustomerAsync(new object[] 
                    {
                        customer.newUserID, 
                        signUpRequest.BirthDate
                    });

                    // Als het koppelen mislukt wordt de transactie geroldbacked
                    if (!personal.status)
                    {
                        transaction.Rollback();
                        commit = false;

                        return BadRequest(new { personal.message });
                    }

                    // Er wordt een welkomsemail verstuurd
                    await _emailService.SendWelcomeEmail(signUpRequest.Email);
                    
                    return Ok(new { message = "Account created successfully. Please check your email to confirm your account" });
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

                return StatusCode(500, new
                {
                    message = ex.Message,
                    details = ex.Message,
                    stackTrace = ex.StackTrace 
                });
            }
            finally
            {
                if (commit)
                {
                    transaction.Commit();
                }
            }
        }
    }
    */

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

    /// <summary>
    /// Een account voor zakelijke gebruikers kan aangemaakt worden (onderscheiden door KvK-nummer)
    /// </summary>
    /// <param name="signUpRequest"></param>
    /// <returns></returns>
    /*[HttpPost("signUpEmployee")]
    public async Task<IActionResult> signUpEmployeeAsync([FromBody] SignUpRequest signUpRequest)
    {
        var connection = _connector.CreateDbConnection();
        var emailCheck = _userRepository.checkUsageEmailAsync(signUpRequest.Email);
        bool commit = true;

        using (var transaction = connection.BeginTransaction())
        {
            try
            {
                // Er wordt gekeken of alle velden zijn ingevuld
                bool filledIn = IsFilledIn(signUpRequest);
                if (!filledIn )
                {
                    return BadRequest(new { message = "Not all elements are filled in" });
                }
                // Er wordt gekeken of het opgegeven emailadress valid is
                if (!EmailChecker.IsValidEmail(signUpRequest.Email))
                {
                    return BadRequest(new { message = "Invalid email format" });
                }

                // er wordt gekeken of het opgegevens KvK nummer in de DataBase staat
                var kvkChecker = new KvkChecker(_userRepository);
                var (isValidKvk, kvkErrorMessage) = await kvkChecker.IsKvkNumberValid(signUpRequest.KvK);

                if (!isValidKvk)
                {
                    return BadRequest(new { message = kvkErrorMessage });
                }
                
                // Er wordt gekeken of het wachtwoord bestaat uit minstens 10 karakters, 1 hoofdletter, 1 kleine letter, 1 cijfer en 1 speciaal teken
                var (isPasswordValid, passwordError) = PasswordChecker.IsValidPassword(signUpRequest.Password);
                if (!isPasswordValid)
                {
                    return BadRequest(new { message = passwordError });
                }

                var emailStatus = await emailCheck;

                // Er wordt gekeken of er een geldig 06-nummer is opgegeven
                if (!TelChecker.IsValidPhoneNumber(signUpRequest.TelNumber))
                {
                    return BadRequest(new { message = "Invalid phone number" });
                }
                else if (emailStatus.status)
                {
                    transaction.Rollback();
                    commit = false;

                    return BadRequest( new { message = "Email already exists"} );
                }
                // Als alle velden zijn ingevuld en het KvK nummer is niet leeg, dan wordt er een nieuwe customer aangemaakt
                else if (filledIn && signUpRequest.KvK != null)
                {
                    // Er wordt in de table UserCustomer een gebruiker aangemaakt
                    var customer = await _userRepository.addCustomerAsync(new object[] 
                    {
                        signUpRequest.Adres,
                        signUpRequest.TelNumber,
                        _hash.createHash(signUpRequest.Password),
                        signUpRequest.Email,
                        signUpRequest.FirstName,
                        signUpRequest.LastName
                    });

                    // Er wordt aan de nieuw aangemaakt customer het opgegeven KvK nummer gekoppeld in de tabel UserEmployee
                    var employee = await _userRepository.addEmployeeCustomerAsync(new object[] 
                    {
                        customer.newUserID,
                        signUpRequest.KvK
                    });

                    // Als het opslaan van de medewerker mislukt wordt te transactie gerollbacked
                    if (!employee.status)
                    {
                        transaction.Rollback();
                        commit = false;

                        return BadRequest(new { employee.status });
                    }

                    // Er wordt een welkomsemail verstuurd
                    await _emailService.SendWelcomeEmail(signUpRequest.Email);
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
                
                return StatusCode(500, new
                {
                    message = ex.Message,
                    details = ex.Message,
                    stackTrace = ex.StackTrace 
                });
            }
            finally
            {
                if (commit)
                {
                    transaction.Commit();
                }
            }
        }    
    }
    */
}