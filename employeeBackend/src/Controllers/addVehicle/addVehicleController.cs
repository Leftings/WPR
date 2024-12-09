namespace Employee.Controllers.AddVehicle;

using Microsoft.AspNetCore.Mvc;
using Employee.Database;
using System;
using MySqlX.XDevAPI.Common;
using Employee.Repository;
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
    public async Task<IActionResult> AddVehicleAsync([FromBody] AddVehicleRequest addVehicleRequest)
    {
        (bool status, string message) result = await _userRepository.AddVehicleAsync(
            addVehicleRequest.YoP,
            addVehicleRequest.Brand,
            addVehicleRequest.Type,
            addVehicleRequest.LicensPlate,
            addVehicleRequest.Color,
            addVehicleRequest.Sort,
            addVehicleRequest.Price,
            addVehicleRequest.Description,
            addVehicleRequest.Vehicleblob
        );

        if (result.status)
        {
            return Ok( new { message = result.message} );
        }
        return BadRequest( new { message = result.message} );
    }
}