using Microsoft.AspNetCore.Mvc;
using WPR.Repository;
using WPR.Cryption;

namespace WPR.Controllers.Employee.VehicleManager.GetInfoVehicleManager;

[Route("api/[controller]")]
[ApiController]
public class GetInfoVehicleManagerController : ControllerBase
{
    // Afhankelijkheden voor het ophalen van gebruikersinformatie en cryptografie
    private readonly IUserRepository _userRepository;
    private readonly Crypt _crypt;

    // Constructor voor het injecteren van de afhankelijkheden
    public GetInfoVehicleManagerController(IUserRepository userRepository, Crypt crypt)
    {
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository)); // Zorg ervoor dat de repository niet null is
        _crypt = crypt ?? throw new ArgumentNullException(nameof(crypt)); // Zorg ervoor dat de crypt niet null is
    }

    /// <summary>
    /// Verkrijgt alle informatie van de voertuigmanager, inclusief bijbehorende klanten met hetzelfde businessnummer.
    /// </summary>
    /// <param name="id">Het ID van de voertuigmanager</param>
    /// <returns>Details van de voertuigmanager en klanten met hetzelfde businessnummer</returns>
    [HttpGet("GetAllInfo")]
    public async Task<IActionResult> GetAllInfo(int id)
    {
        try
        {
            // Valideer dat het ID van de voertuigmanager groter is dan 0
            if (id <= 0)
            {
                return BadRequest(new { message = "Invalid vehicle manager ID" }); // Foutmelding voor ongeldig ID
            }

            // Haal de informatie van de voertuigmanager op
            var vehicleManagerInfo = await _userRepository.GetVehicleManagerInfoAsync(id);

            // Als de voertuigmanager niet gevonden wordt, stuur dan een 404
            if (vehicleManagerInfo == null)
            {
                return NotFound(new { message = "Vehicle manager not found" });
            }

            var businessNumber = vehicleManagerInfo.Business;
            // Als het businessnummer leeg is, stuur dan een foutmelding
            if (string.IsNullOrEmpty(businessNumber))
            {
                return BadRequest(new { message = "Vehicle manager does not have a valid business number" });
            }

            // Haal de klanten op die aan dit businessnummer gekoppeld zijn
            var customers = await _userRepository.GetCustomersByBusinessNumberAsync(businessNumber);

            // Stuur een succesvolle response met voertuigmanager informatie en bijbehorende klanten
            return Ok(new
            {
                message = "Success",
                vehicleManagerInfo = new
                {
                    vehicleManagerInfo.Id,
                    vehicleManagerInfo.Email,
                    vehicleManagerInfo.Business,
                    vehicleManagerInfo.Password
                },
                customers = customers?.Select(c => new { c.Id, c.Email, c.Kvk }) ?? Enumerable.Empty<object>()
            });
        }
        catch (Exception ex)
        {
            // Log en stuur een serverfoutmelding als er een uitzondering optreedt
            Console.WriteLine($"Error in GetAllInfo: {ex.Message}");
            return StatusCode(500, new { message = "Internal server error", error = ex.Message });
        }
    }

    /// <summary>
    /// Werk het e-mailadres en wachtwoord van de voertuigmanager bij.
    /// </summary>
    [HttpPut("UpdateCustomer")]
    public async Task<IActionResult> UpdateCustomer([FromQuery] int id, [FromBody] CustomerUpdateRequest updates)
    {
        try
        {
            // Controleer of het klant-ID geldig is
            if (id <= 0)
            {
                return BadRequest(new { message = "Invalid customer ID" });
            }

            // Controleer of de updates (e-mail en wachtwoord) niet null of leeg zijn
            if (updates == null || string.IsNullOrEmpty(updates.Email) || string.IsNullOrEmpty(updates.Password))
            {
                return BadRequest(new { message = "Email and Password cannot be empty" });
            }

            var password = updates.Password;

            // Probeer de klantgegevens bij te werken in de database
            var updateResult = await _userRepository.UpdateCustomerAsync(id, updates.Email, password);

            if (!updateResult)
            {
                return StatusCode(500, new { message = "Failed to update customer information" });
            }

            return Ok(new { message = "Customer info updated successfully" });
        }
        catch (Exception ex)
        {
            // Log de fout en stuur een serverfoutmelding terug
            return StatusCode(500, new { message = "Internal server error", error = ex.Message });
        }
    }

    /// <summary>
    /// Verkrijgt klanten die aan een specifiek businessnummer gekoppeld zijn.
    /// </summary>
    [HttpGet("GetCustomersByBusinessNumber")]
    public async Task<IActionResult> GetCustomersByBusinessNumber(string businessNumber)
    {
        try
        {
            // Controleer of het businessnummer geldig is
            if (string.IsNullOrEmpty(businessNumber))
            {
                return BadRequest(new { message = "Business number cannot be empty" });
            }

            // Haal klanten op die aan het businessnummer gekoppeld zijn
            var customers = await _userRepository.GetCustomersByBusinessNumberAsync(businessNumber);

            // Als er geen klanten gevonden worden, stuur dan een foutmelding
            if (customers == null || !customers.Any())
            {
                return NotFound(new { message = "No customers found for this business" });
            }

            // Retourneer een succesvolle response met klanten
            return Ok(new
            {
                message = "Success",
                customers = customers.Select(c => new { c.Id, c.Email, c.Kvk })
            });
        }
        catch (Exception ex)
        {
            // Log de fout en stuur een serverfoutmelding terug
            Console.WriteLine($"Error in GetCustomersByBusinessNumber: {ex.Message}");
            return StatusCode(500, new { message = "Internal server error", error = ex.Message });
        }
    }

    // Model voor de klantupdateverzoeken (e-mail en wachtwoord)
    public class CustomerUpdateRequest
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }
    
    /// <summary>
    /// Verkrijgt het domein van een bedrijf aan de hand van het KvK-nummer.
    /// </summary>
    /// <param name="kvk">Het KvK-nummer van het bedrijf</param>
    /// <returns>Het bedrijfsdomein</returns>
    [HttpGet("GetBusinessDomainByKvK")]
    public async Task<IActionResult> GetBusinessDomainByKvK(int kvk)
    {
        try
        {
            // Controleer of het KvK-nummer geldig is
            if (kvk <= 0)
            {
                return BadRequest(new { message = "Invalid KvK" });
            }

            // Verkrijg het domein van het bedrijf aan de hand van het KvK-nummer
            var (statusCode, domain) = await _userRepository.GetBusinessDomainByKvK(kvk);

            // Als het domein niet gevonden wordt, stuur een 404
            if (statusCode == 404)
            {
                return NotFound(new { message = "Business domain not found for the given KvK" });
            }
            // Als er een serverfout is, stuur een 500 met foutmelding
            else if (statusCode == 500)
            {
                return StatusCode(500, new { message = "Internal server error", error = domain });
            }

            // Retourneer het domein van het bedrijf als de aanvraag succesvol is
            return Ok(new
            {
                message = "Success",
                domain = domain
            });
        }
        catch (Exception ex)
        {
            // Log de fout en stuur een serverfoutmelding terug
            Console.WriteLine($"Error in GetBusinessDomainByKvK: {ex.Message}");
            return StatusCode(500, new { message = "Internal server error", error = ex.Message });
        }
    }
}
