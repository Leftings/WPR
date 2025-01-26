using Microsoft.AspNetCore.Mvc;
using WPR.Controllers.General.Cookie;
using WPR.Cryption;
using WPR.Database;
using WPR.Repository;

namespace WPR.Controllers.Employee;

/// <summary>
/// EmployeeController is de controller die ervoor zorgt dat er gekeken wordt of de ingelogde gebruiker een medewerker is
/// </summary>
[Route("api/[controller]")]
[ApiController]
public class EmployeeController : ControllerBase
{
    private readonly IUserRepository _userRepository;
    private readonly Connector _connector;
    private readonly SessionHandler _sessionHandler;
    private readonly Crypt _crypt;

    // Constructor die de benodigde services injecteert
    public EmployeeController(IUserRepository userRepository, Connector connector, Crypt crypt, SessionHandler sessionHandler)
    {
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _connector = connector ?? throw new ArgumentNullException(nameof(connector));
        _sessionHandler = sessionHandler ?? throw new ArgumentNullException(nameof(sessionHandler));
        _crypt = crypt ?? throw new ArgumentNullException(nameof(crypt));
    }

    /// <summary>
    /// Deze methode controleert of de gebruiker die momenteel is ingelogd een medewerker is.
    /// Het haalt de 'LoginSession' cookie op en probeert de user te identificeren.
    /// </summary>
    /// <returns>Een boolean waarde die aangeeft of de gebruiker een medewerker is</returns>
    [HttpGet("IsUserEmployee")]
    public Task<bool> IsUserEmployee()
    {
        // Verkrijg de login cookie
        string loginCookie = HttpContext.Request.Cookies["LoginSession"];
        int id;
        
        // Als de login cookie leeg of null is, geef dan terug dat de gebruiker geen medewerker is
        if (string.IsNullOrEmpty(loginCookie))
        {
            return _userRepository.IsUserEmployee(-1);  // -1 betekent geen geldige gebruiker
        }

        try
        {
            // Probeer de cookie te decrypten en om te zetten naar een ID
            id = Convert.ToInt32(_crypt.Decrypt(loginCookie));
            // Controleer of de gebruiker met het gedecryptte ID een medewerker is
            return _userRepository.IsUserEmployee(id);
        }
        catch
        {
            // Als het decrypten van de cookie mislukt, voeg een ongeldige cookie toe
            Response.Cookies.Append("LoginSession", "Invalid cookie", new CookieOptions
            {
                HttpOnly = true, // Alleen toegankelijk via HTTP (niet via JavaScript)
                Secure = true,   // Alleen via HTTPS
                Expires = DateTimeOffset.UtcNow.AddDays(-1)  // Zet de vervaldatum van de cookie in het verleden
            });

            // Als er een fout is, retourneer dan dat de gebruiker geen medewerker is
            return _userRepository.IsUserEmployee(-1);
        }
    }
}
