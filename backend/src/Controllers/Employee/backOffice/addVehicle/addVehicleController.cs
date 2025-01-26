namespace WPR.Controllers.Employee.BackOffice.AddVehicle;

using Microsoft.AspNetCore.Mvc;
using WPR.Database;
using System;
using WPR.Repository;

/// <summary>
/// AddVehicleController zorgt ervoor dat voertuigen toegevoegd kunnen worden aan de database
/// </summary>
[Route("api/[controller]")]
[ApiController]
public class AddVehicleController : ControllerBase
{
    private readonly IConnector _connector;
    private readonly IEmployeeRepository _userRepository;

    public AddVehicleController(IConnector connector, IEmployeeRepository userRepository)
    {
        _connector = connector ?? throw new ArgumentNullException(nameof(connector));
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
    }

    /// <summary>
    /// Er kan een voertuig toegevoegd worden
    /// </summary>
    /// <param name="request"></param>
    /// <param name="vehicleBlob"></param>
    /// <returns></returns>
    [HttpPost("addVehicle")]
    // Niet te testen via Swagger, wegens het toevoegen van een afbeelding
    [ApiExplorerSettings(IgnoreApi = true)]
    public async Task<IActionResult> AddVehicleAsync([FromForm] AddVehicleRequest request, [FromForm] IFormFile vehicleBlob)
    {
        try
        {
            // Vehicle blob wordt omgezet naar bytes
            byte[] vehicleBlobBytes = null;

            if (vehicleBlob != null)
            {
                using (var memoryStream = new MemoryStream())
                {
                    await vehicleBlob.CopyToAsync(memoryStream);
                    vehicleBlobBytes = memoryStream.ToArray();
                }
            }

            // Gegevens worden doorgestuurd naar de userRepository
            var status = await _userRepository.AddVehicleAsync(
                request.YoP,
                request.Brand,
                request.Type,
                request.LicensePlate,
                request.Color,
                request.Sort,
                request.Price,
                request.Description,
                vehicleBlobBytes,
                request.Places);

            if (status.status)
            {
                return Ok(new AddVehicleResponse{ Message = status.message });
            }
            return BadRequest(new AddVehicleResponse{ Message = status.message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { status = false, message = ex.Message });
        }
    }

}