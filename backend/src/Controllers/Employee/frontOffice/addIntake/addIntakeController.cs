namespace WPR.Controllers.Employee.FrontOffice.AddIntake;

using Microsoft.AspNetCore.Mvc;
using WPR.Database;
using System;
using MySqlX.XDevAPI.Common;
using WPR.Repository;
using MySql.Data.MySqlClient;

/// <summary>
/// AddIntakeController zorgt ervoor dat informatie over de inname van een voertuig toegevoegd kan worden aan de database
/// </summary>
[Route("api/[controller]")]
[ApiController]
public class AddIntakeController : ControllerBase
{
    private readonly Connector _connector;
    private readonly IEmployeeRepository _employeeRepository;
    private readonly IVehicleRepository _vehicleRepository;

    public AddIntakeController(Connector connector, IEmployeeRepository employeeRepository, IVehicleRepository vehicleRepository)
    {
        _connector = connector ?? throw new ArgumentNullException(nameof(connector));
        _employeeRepository = employeeRepository ?? throw new ArgumentNullException(nameof(employeeRepository));
        _vehicleRepository = vehicleRepository ?? throw new ArgumentNullException(nameof(vehicleRepository));
    }

    /// <summary>
    /// Er kan een inname toegevoegd worden
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost("addIntake")]
    public async Task<IActionResult> AddIntakeAsync([FromForm] AddIntakeRequest request)
    {
        try
        {

            // Gegevens worden doorgestuurd naar de userRepository
            var status = await _employeeRepository.AddIntakeAsync(
                request.Damage,
                request.FrameNrVehicle,
                request.ReviewedBy,
                request.Date,
                request.Contract);

            if (!request.Damage.Equals("Geen schade aanwezig."))
            {
                var status2 = _vehicleRepository.ChangeRepairStatus(request.FrameNrVehicle, true);
            }

            if (status.status)
            {
                return Ok(new AddIntakeResponse { Message = status.message });
            }
            return BadRequest(new AddIntakeErrorResponse
            {
                Status = false,
                Message = status.message
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { status = false, message = ex.Message });
        }
    }

}