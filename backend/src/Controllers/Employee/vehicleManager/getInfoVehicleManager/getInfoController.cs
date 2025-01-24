using Microsoft.AspNetCore.Mvc;
using WPR.Repository;

namespace WPR.Controllers.Employee.VehicleManager.GetInfoVehicleManager;

[Route("api/[controller]")]
[ApiController]
public class GetInfoVehicleManagerController : ControllerBase
{
    private readonly IUserRepository _userRepository; // Updated repository

    public GetInfoVehicleManagerController(IUserRepository userRepository)
    {
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
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
            // Fetch vehicle manager info
            var vehicleManagerInfo = await _userRepository.GetVehicleManagerInfoAsync(id);

            if (vehicleManagerInfo == null)
            {
                return NotFound(new { message = "Vehicle manager not found" });
            }

            // Extract business number from the vehicle manager info
            var businessNumber = vehicleManagerInfo.Business;
            if (string.IsNullOrEmpty(businessNumber))
            {
                return BadRequest(new { message = "Invalid or missing business number" });
            }

            Console.WriteLine($"Vehicle Manager Business Number: {businessNumber}");

            // Fetch customers linked to the same business number
            var customers = await _userRepository.GetCustomersByBusinessNumberAsync(businessNumber);

            if (customers == null || customers.Count == 0)
            {
                Console.WriteLine("No customers found for this business number.");
            }
            else
            {
                Console.WriteLine($"Found {customers.Count} customer(s) for business number {businessNumber}.");
            }

            return Ok(new
            {
                message = "Success",
                vehicleManagerInfo = new
                {
                    vehicleManagerInfo.Id,
                    vehicleManagerInfo.Email,
                    vehicleManagerInfo.Business
                },
                customers = customers.Select(c => new { c.Id, c.Email, c.Kvk }) // Return email and Kvk
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            return StatusCode(500, new { message = "Internal server error", error = ex.Message });
        }
    }



    /// <summary>
    /// Updates the vehicle manager's email and password.
    /// </summary>
    [HttpPut("UpdateVehicleManager")]
    public async Task<IActionResult> UpdateVehicleManager(int id, [FromBody] Dictionary<string, string> updates)
    {
        try
        {
            // Validate input
            if (!updates.ContainsKey("Email") || !updates.ContainsKey("Password"))
            {
                return BadRequest(new { message = "Missing required fields" });
            }

            var email = updates["Email"];
            var password = updates["Password"];

            // Directly update the password without hashing or encryption
            var updateResult = await _userRepository.UpdateVehicleManagerAsync(id, email, password);

            if (!updateResult)
            {
                return StatusCode(500, new { message = "Failed to update vehicle manager information" });
            }

            return Ok(new { message = "Vehicle manager info updated successfully" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Internal server error", error = ex.Message });
        }
    }
    [HttpGet("GetCustomersByBusinessNumber")]
    public async Task<IActionResult> GetCustomersByBusinessNumber(string businessNumber)
    {
        try
        {
            var customers = await _userRepository.GetCustomersByBusinessNumberAsync(businessNumber);
            if (customers == null || !customers.Any())
            {
                return NotFound(new { message = "No customers found for this business" });
            }
            return Ok(new { data = customers });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Internal server error", error = ex.Message });
        }
    }

}
