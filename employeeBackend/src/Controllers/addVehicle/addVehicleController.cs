namespace Employee.Controllers.AddVehicle;

using Microsoft.AspNetCore.Mvc;
using Employee.Database;
using System;
using MySqlX.XDevAPI.Common;
using Employee.Repository;
using MySql.Data.MySqlClient;

[Route("api/[controller]")]
[ApiController]
public class AddVehicleController : ControllerBase
{
    private readonly Connector _connector;
    private readonly IUserRepository _userRepository;

    public AddVehicleController(Connector connector, IUserRepository userRepository)
    {
        _connector = connector ?? throw new ArgumentNullException(nameof(connector));
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
    }

    [HttpPost("addVehicle")]
    public async Task<IActionResult> AddVehicleAsync([FromForm] AddVehicleRequest request, [FromForm] IFormFile vehicleBlob)
    {
        try
        {
            byte[] vehicleBlobBytes = null;
            
            // Read the file into a byte array
            if (vehicleBlob != null)
            {
                using (var memoryStream = new MemoryStream())
                {
                    await vehicleBlob.CopyToAsync(memoryStream);
                    vehicleBlobBytes = memoryStream.ToArray();
                }
            }

            var status = await _userRepository.AddVehicleAsync(
            request.YoP,
            request.Brand,
            request.Type,
            request.LicensePlate,
            request.Color,
            request.Sort,
            request.Price,
            request.Description,
            vehicleBlobBytes );

            if (status.status)
            {
                return Ok(new { status.message });
            }
            return BadRequest(new { status.message });
        } catch(Exception ex)
        {
            return StatusCode(500, new { status = false, message = ex.Message });
        }
    }
}