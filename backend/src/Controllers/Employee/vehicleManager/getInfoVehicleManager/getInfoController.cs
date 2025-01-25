using Microsoft.AspNetCore.Mvc;
using WPR.Repository;
using WPR.Cryption;

namespace WPR.Controllers.Employee.VehicleManager.GetInfoVehicleManager;

[Route("api/[controller]")]
[ApiController]
public class GetInfoVehicleManagerController : ControllerBase
{
    private readonly IUserRepository _userRepository;
    private readonly Crypt _crypt;

    public GetInfoVehicleManagerController(IUserRepository userRepository, Crypt crypt)
    {
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _crypt = crypt ?? throw new ArgumentNullException(nameof(crypt));
    }

    /// <summary>
    /// Get all vehicle manager info, including associated customers with the same business number.
    /// </summary>
    /// <param name="id">The vehicle manager's ID</param>
    /// <returns>Vehicle manager details and customers with the same business number</returns>
    [HttpGet("GetAllInfo")]
    public async Task<IActionResult> GetAllInfo(int id)
    {
        try
        {
            if (id <= 0)
            {
                return BadRequest(new { message = "Invalid vehicle manager ID" });
            }

            var vehicleManagerInfo = await _userRepository.GetVehicleManagerInfoAsync(id);

            if (vehicleManagerInfo == null)
            {
                return NotFound(new { message = "Vehicle manager not found" });
            }

            var businessNumber = vehicleManagerInfo.Business;
            if (string.IsNullOrEmpty(businessNumber))
            {
                return BadRequest(new { message = "Vehicle manager does not have a valid business number" });
            }

            var customers = await _userRepository.GetCustomersByBusinessNumberAsync(businessNumber);

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
            Console.WriteLine($"Error in GetAllInfo: {ex.Message}");
            return StatusCode(500, new { message = "Internal server error", error = ex.Message });
        }
    }

    /// <summary>
    /// Updates the vehicle manager's email and password.
    /// </summary>
    [HttpPut("UpdateCustomer")]
    public async Task<IActionResult> UpdateCustomer([FromQuery] int id, [FromBody] CustomerUpdateRequest updates)
    {
        try
        {
            if (id <= 0)
            {
                return BadRequest(new { message = "Invalid customer ID" });
            }

            if (updates == null || string.IsNullOrEmpty(updates.Email) || string.IsNullOrEmpty(updates.Password))
            {
                return BadRequest(new { message = "Email and Password cannot be empty" });
            }

            var password = updates.Password;

            var updateResult = await _userRepository.UpdateCustomerAsync(id, updates.Email, password);

            if (!updateResult)
            {
                return StatusCode(500, new { message = "Failed to update customer information" });
            }

            return Ok(new { message = "Customer info updated successfully" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Internal server error", error = ex.Message });
        }
    }



    /// <summary>
    /// Get customers associated with a specific business number.
    /// </summary>
    [HttpGet("GetCustomersByBusinessNumber")]
    public async Task<IActionResult> GetCustomersByBusinessNumber(string businessNumber)
    {
        try
        {
            if (string.IsNullOrEmpty(businessNumber))
            {
                return BadRequest(new { message = "Business number cannot be empty" });
            }

            var customers = await _userRepository.GetCustomersByBusinessNumberAsync(businessNumber);

            if (customers == null || !customers.Any())
            {
                return NotFound(new { message = "No customers found for this business" });
            }

            return Ok(new
            {
                message = "Success",
                customers = customers.Select(c => new { c.Id, c.Email, c.Kvk })
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in GetCustomersByBusinessNumber: {ex.Message}");
            return StatusCode(500, new { message = "Internal server error", error = ex.Message });
        }
    }
    public class CustomerUpdateRequest
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }

}