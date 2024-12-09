namespace Employee.Controllers.AddVehicle;

using Microsoft.AspNetCore.Mvc;
using Employee.Database;
using System;
using MySqlX.XDevAPI.Common;
using Employee.Repository;
using Employee.Cryption;

[Route("api/[controller]")]
[ApiController]
public class CookieController : ControllerBase
{
    private readonly Connector _connector;
    private readonly IUserRepository _userRepository;
    private readonly Crypt _crypt;

    public CookieController(Connector connector, IUserRepository userRepository, Crypt crypt)
    {
        _connector = connector ?? throw new ArgumentNullException(nameof(connector));
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _crypt = crypt ?? throw new ArgumentNullException(nameof(crypt));
    }

    [HttpGet("AddVehicle")]
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