using Microsoft.AspNetCore.Mvc;
using WPR.Cryption;
using WPR.Repository;
using WPR.Utils;

namespace WPR.Controllers.Employee.VehicleManager.GetInfoVehicleManager;

[Route("api/[controller]")]
[ApiController]
public class GetInfoVehicleManagerController : ControllerBase
{
    private readonly IEmployeeRepository _employeeRepository;

    public GetInfoVehicleManagerController(IEmployeeRepository employeeRepository)
    {
        _employeeRepository = employeeRepository ?? throw new ArgumentNullException(nameof(employeeRepository));
    }

    [HttpGet("GetAllInfo")]
    public async Task<IActionResult> GetAllInfo(int id)
    {
        (int StatusCode, string Message, Dictionary<string, object> Data) response = _employeeRepository.GetVehicleManagerInfo(id);

        return StatusCode(response.StatusCode, new { message = response.Message, data = response.Data });
    }
}

