using Microsoft.AspNetCore.Mvc;
using WPR.Repository;
namespace WPR.Controllers.Customer.Rental;

[Route("api/[controller]")]
[ApiController]
public class RentalController : ControllerBase
{
    private readonly VehicleRepository _vehicleRepo;

    public RentalController(VehicleRepository vehicleRepo)
    {
        _vehicleRepo = vehicleRepo ?? throw new ArgumentNullException(nameof(vehicleRepo));
    }


    [HttpDelete("CancelRental")]
    public async Task<IActionResult> CancelRentalAsync(int rentalId)
    {
        string loginCookie = HttpContext.Request.Cookies["LoginSession"];
        (bool Status, int StatusCode, string Message) response = await _vehicleRepo.CancelRental(rentalId, loginCookie);

        return StatusCode(response.StatusCode, new { message = response.Message });
    }


    [HttpGet("GetAllUserRentals")]
    public async Task<IActionResult> GetAllUserRentalsAsync()
    {
        string loginCookie = HttpContext.Request.Cookies["LoginSession"];
        (bool Status, int StatusCode, string Message, IList<object> Rentals) response = await _vehicleRepo.GetAllUserRentals(loginCookie);
        
        if (response.Status)
        {
            return StatusCode(response.StatusCode, response.Rentals);
        }
        return StatusCode(response.StatusCode, new { message = response.Message });
    }

    [HttpGet("GetAllUserRentalsWithDetails")]
    public async Task<IActionResult> GetAllUserRentalsWithDetailsAsync()
    {
        (bool Status, int StatusCode, string Message, IList<object> UserRentals) response = _vehicleRepo.GetAllUserRentalsDetailed();

        if (response.Status)
        {
            return StatusCode(response.StatusCode, response.UserRentals);
        }
        return StatusCode(response.StatusCode, new { message = response.Message });
    }
    
    [HttpPut("ChangeRental")]
    public async Task<IActionResult> ChangeRentalAsync([FromBody] UpdateRentalRequest request)
    {
        (bool Status, int StatusCode, string Message) response = _vehicleRepo.ChangeRental(request);
        return StatusCode(response.StatusCode, new { message = response.Message });
    }

    [HttpPost("CreateRental")]
    public async Task<IActionResult> CreateRental([FromBody] RentalRequest request)
    {
        string userId = Request.Cookies["LoginSession"];

        (bool Status, string Message) result = await _vehicleRepo.HireVehicle(request, userId);
        
        if (result.Status)
        {
            return Ok( new { message = result.Message});
        }
        return BadRequest( new { message = result.Message });
    }
}
