using Microsoft.AspNetCore.Mvc;
using WPR.Repository;

namespace WPR.Controllers.Customer.Rental;

[Route("api/[controller]")]
[ApiController]
public class RentalController : ControllerBase
{
    private readonly VehicleRepository _vehicleRepo;

    // Constructor ontvangt de VehicleRepository voor interactie met de voertuigdata.
    public RentalController(VehicleRepository vehicleRepo)
    {
        _vehicleRepo = vehicleRepo ?? throw new ArgumentNullException(nameof(vehicleRepo));
    }

    /// <summary>
    /// Annuleert een bestaande verhuurtransactie op basis van het opgegeven rentalId.
    /// </summary>
    /// <param name="rentalId">Het ID van de verhuur dat geannuleerd moet worden.</param>
    /// <returns>Een HTTP-resultaat met de status van de annulering.</returns>
    [HttpDelete("CancelRental")]
    public async Task<IActionResult> CancelRentalAsync(int rentalId)
    {
        // Verkrijg het login cookie om de gebruiker te identificeren.
        string loginCookie = HttpContext.Request.Cookies["LoginSession"];
        
        // Roep de repository aan om de verhuur te annuleren.
        (bool Status, int StatusCode, string Message) response = await _vehicleRepo.CancelRental(rentalId, loginCookie);

        // Retourneer de status van de annulering.
        return StatusCode(response.StatusCode, new { message = response.Message });
    }

    /// <summary>
    /// Haalt alle verhuurtransacties van de ingelogde gebruiker op.
    /// </summary>
    /// <returns>Een lijst van alle verhuurtransacties van de gebruiker.</returns>
    [HttpGet("GetAllUserRentals")]
    public async Task<IActionResult> GetAllUserRentalsAsync()
    {
        // Verkrijg het login cookie van de gebruiker.
        string loginCookie = HttpContext.Request.Cookies["LoginSession"];
        
        // Roep de repository aan om de verhuurtransacties van de gebruiker op te halen.
        (bool Status, int StatusCode, string Message, IList<object> Rentals) response = await _vehicleRepo.GetAllUserRentals(loginCookie);
        
        // Als de status succesvol is, retourneer de lijst met verhuur.
        if (response.Status)
        {
            return StatusCode(response.StatusCode, response.Rentals);
        }

        // Als er iets misgaat, stuur dan een foutmelding.
        return StatusCode(response.StatusCode, new { message = response.Message });
    }

    /// <summary>
    /// Haalt alle verhuurtransacties van de gebruiker op, inclusief gedetailleerde informatie.
    /// </summary>
    /// <returns>Een lijst van gedetailleerde verhuurtransacties.</returns>
    [HttpGet("GetAllUserRentalsWithDetails")]
    public async Task<IActionResult> GetAllUserRentalsWithDetailsAsync()
    {
        // Roep de repository aan om gedetailleerde verhuurtransacties op te halen.
        (bool Status, int StatusCode, string Message, IList<object> UserRentals) response = _vehicleRepo.GetAllUserRentalsDetailed();

        // Als de status succesvol is, retourneer de gedetailleerde verhuur.
        if (response.Status)
        {
            return StatusCode(response.StatusCode, response.UserRentals);
        }

        // Als er iets misgaat, stuur dan een foutmelding.
        return StatusCode(response.StatusCode, new { message = response.Message });
    }

    /// <summary>
    /// Wijzigt een bestaande verhuur op basis van de opgegeven gegevens.
    /// </summary>
    /// <param name="request">De gegevens voor het bijwerken van de verhuur.</param>
    /// <returns>Een HTTP-resultaat met de status van de wijziging.</returns>
    [HttpPut("ChangeRental")]
    public async Task<IActionResult> ChangeRentalAsync([FromBody] UpdateRentalRequest request)
    {
        // Roep de repository aan om de verhuur te wijzigen.
        (bool Status, int StatusCode, string Message) response = _vehicleRepo.ChangeRental(request);

        // Retourneer de status van de wijziging.
        return StatusCode(response.StatusCode, new { message = response.Message });
    }

    /// <summary>
    /// Maakt een nieuwe verhuurtransactie aan voor een voertuig.
    /// </summary>
    /// <param name="request">De gegevens voor de nieuwe verhuur.</param>
    /// <returns>Een HTTP-resultaat met de status van de verhuur.</returns>
    [HttpPost("CreateRental")]
    public async Task<IActionResult> CreateRental([FromBody] RentalRequest request)
    {
        // Verkrijg het login cookie van de gebruiker.
        string userId = Request.Cookies["LoginSession"];

        // Roep de repository aan om de verhuur aan te maken.
        (bool Status, string Message) result = await _vehicleRepo.HireVehicle(request, userId);
        
        // Als de verhuur succesvol is, stuur dan een succesbericht.
        if (result.Status)
        {
            return Ok(new { message = result.Message });
        }

        // Als de verhuur niet succesvol is, stuur dan een foutmelding.
        return BadRequest(new { message = result.Message });
    }
}
