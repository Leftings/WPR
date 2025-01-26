using WPR.Cryption;

namespace WPR.Controllers.Customer.ChangeUserSettings;

using Microsoft.AspNetCore.Mvc;
using WPR.Database;
using System;
using WPR.Repository;
using WPR.Repository.DatabaseCheckRepository;

/// <summary>
/// ChangeUserSettingsController is de controller die aanvragen van buitenaf ontvangt en vervolgens gegevens uit de backend ophaalt en teruggeeft.
/// De controller behandelt het wijzigen van gebruikersinformatie en het verwijderen van een gebruiker.
/// </summary>
[Route("api/[controller]")]
[ApiController]
public class ChangeUserSettingsController : ControllerBase
{
    private readonly Connector _connector;
    private readonly IUserRepository _userRepository;
    private readonly Crypt _crypt;
    private readonly IDatabaseCheckRepository _databaseCheckRepository;

    // Constructor die de benodigde services en repositories injecteert.
    public ChangeUserSettingsController(Connector connector, IUserRepository userRepository, Crypt crypt, IDatabaseCheckRepository databaseCheckRepository)
    {
        _connector = connector ?? throw new ArgumentNullException(nameof(connector));
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _crypt = crypt ?? throw new ArgumentNullException(nameof(crypt));
        _databaseCheckRepository = databaseCheckRepository ?? throw new ArgumentNullException(nameof(databaseCheckRepository));
    }

    /// <summary>
    /// Wijzigt de gebruikersgegevens door alleen de gewijzigde velden naar de backend te sturen.
    /// </summary>
    /// <param name="changeUserRequest">De gegevens die de gebruiker heeft gewijzigd.</param>
    /// <returns>Een HTTP-resultaat dat aangeeft of de wijziging succesvol was.</returns>
    [HttpPut("ChangeUserInfo")]
    public async Task<IActionResult> ChangeUserInfoAsync([FromBody] ChangeUserRequest changeUserRequest)
    {
        // Als het aanvraagobject null is, stuur dan een foutmelding.
        if (changeUserRequest == null)
        {
            return BadRequest("Body can not be null");
        }

        List<object[]> data = new List<object[]>();

        // Loop door alle properties van het ChangeUserRequest-object en voeg gewijzigde waarden toe aan de lijst.
        foreach (var propertyInfo in typeof(ChangeUserRequest).GetProperties())
        {
            var propertyName = propertyInfo.Name;
            var propertyValue = propertyInfo.GetValue(changeUserRequest);
            var propertyType = propertyInfo.PropertyType;

            // Voeg alleen niet-lege waarden toe.
            if (!propertyValue.Equals(""))
            {
                data.Add(new object[] { propertyName, propertyValue, propertyType });
            }
        }

        // Roep de repository aan om de gebruikersinformatie te updaten.
        var updated = await _userRepository.EditUserInfoAsync(data);

        // Als de update succesvol is, stuur dan een succesbericht.
        if (updated.status)
        {
            return Ok(new { message = "Data Updated" });
        }

        // Als er iets misgaat, stuur dan de foutmelding.
        return BadRequest(new { updated.message });
    }

    /// <summary>
    /// Laat gebruikers hun account, samen met hun gegevens, verwijderen uit het systeem.
    /// </summary>
    /// <returns>Een HTTP-resultaat dat aangeeft of het verwijderen succesvol was.</returns>
    [HttpDelete("DeleteUser")]
    public async Task<IActionResult> DeleteUserAsync()
    {
        // Verkrijg het login cookie uit de request.
        string loginCookie = HttpContext.Request.Cookies["LoginSession"];

        // Als er geen cookie is, stuur dan een foutmelding.
        if (string.IsNullOrEmpty(loginCookie))
        {
            Console.WriteLine("No cookie");
            return BadRequest(new { message = "No Cookie" });
        }

        Console.WriteLine(loginCookie);

        try
        {
            // Decrypt de logincookie om de gebruikersinformatie te verkrijgen.
            string decryptedLoginCookie = _crypt.Decrypt(loginCookie);
            Console.WriteLine(decryptedLoginCookie);

            // Roep de repository aan om de gebruiker te verwijderen.
            var deleteUser = _databaseCheckRepository.DeleteUser(Convert.ToInt32(decryptedLoginCookie));

            // Retourneer het resultaat van de deletebewerking.
            return StatusCode(deleteUser.StatusCode, new { message = deleteUser.Message });
        }
        catch (OverflowException ex)
        {
            Console.WriteLine(ex.Message);
            throw;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}
