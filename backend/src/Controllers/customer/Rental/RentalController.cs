using Microsoft.AspNetCore.Mvc;
using WPR.Repository;
namespace WPR.Controllers.Customer.Rental;

[Route("api/[controller]")]
[ApiController]
public class RentalController : ControllerBase
{
    private readonly IVehicleRepository _vehicleRepo;

    public RentalController(IVehicleRepository vehicleRepo)
    {
        _vehicleRepo = vehicleRepo ?? throw new ArgumentNullException(nameof(vehicleRepo));
    }

    /// <summary>
    /// Annuleert een bestaande huur op basis van het opgegeven huur-ID.
    /// </summary>
    /// <param name="rentalId">Het ID van de huur die geannuleerd moet worden.</param>
    /// <returns>Een actie-resultaat met de status van de annulering.</returns>
    [HttpDelete("CancelRental")]
    public async Task<IActionResult> CancelRentalAsync(int rentalId)
    {
        string loginCookie = HttpContext.Request.Cookies["LoginSession"];
        (bool Status, int StatusCode, string Message) response = await _vehicleRepo.CancelRental(rentalId, loginCookie);

        return StatusCode(response.StatusCode, new RentalResponse { Message = response.Message });
    }

    /// <summary>
    /// Haalt alle verhuurrecords op voor de ingelogde gebruiker.
    /// </summary>
    /// <returns>Een lijst met alle huurtransacties van de gebruiker.</returns>
    [HttpGet("GetAllUserRentals")]
    public async Task<IActionResult> GetAllUserRentalsAsync()
    {
        string loginCookie = HttpContext.Request.Cookies["LoginSession"];
        (bool Status, int StatusCode, string Message, IList<object> Rentals) response = await _vehicleRepo.GetAllUserRentals(loginCookie);
        
        if (response.Status)
        {
            return StatusCode(response.StatusCode, response.Rentals);
        }
        return StatusCode(response.StatusCode, new RentalResponse { Message = response.Message });
    }

    /// <summary>
    /// Haalt alle verhuurdetails op van de gebruiker, inclusief aanvullende informatie.
    /// </summary>
    /// <returns>Een lijst met gedetailleerde verhuurrecords van de gebruiker.</returns>
    [HttpGet("GetAllUserRentalsWithDetails")]
    public async Task<IActionResult> GetAllUserRentalsWithDetailsAsync()
    {
        (bool Status, int StatusCode, string Message, IList<object> UserRentals) response = _vehicleRepo.GetAllUserRentalsDetailed();

        if (response.Status)
        {
            return StatusCode(response.StatusCode, response.UserRentals);
        }
        return StatusCode(response.StatusCode, new RentalResponse { Message = response.Message });
    }

    /// <summary>
    /// Wijzigt de gegevens van een bestaande huurtransactie.
    /// </summary>
    /// <param name="request">De gegevens van de huur die gewijzigd moeten worden.</param>
    /// <returns>Een actie-resultaat met de status van de wijziging.</returns>
    [HttpPut("ChangeRental")]
    public async Task<IActionResult> ChangeRentalAsync([FromBody] UpdateRentalRequest request)
    {
        (bool Status, int StatusCode, string Message) response = _vehicleRepo.ChangeRental(request);
        return StatusCode(response.StatusCode, new RentalResponse { Message = response.Message });
    }

    /// <summary>
    /// Maakt een nieuwe huur aan voor een voertuig op basis van de opgegeven gegevens.
    /// </summary>
    /// <param name="request">De gegevens van de huurtransactie, zoals voertuig, datums en prijs.</param>
    /// <returns>Een actie-resultaat met de status van de huurcreatie.</returns>
    [HttpPost("CreateRental")]
    public async Task<IActionResult> CreateRental([FromBody] RentalRequest request)
    {
        string userId = Request.Cookies["LoginSession"];

        (bool Status, string Message) result = await _vehicleRepo.HireVehicle(request, userId);
        
        if (result.Status)
        {
            return Ok(new RentalResponse { Message = result.Message });
        }
        return BadRequest(new RentalResponse { Message = result.Message });
    }
}
