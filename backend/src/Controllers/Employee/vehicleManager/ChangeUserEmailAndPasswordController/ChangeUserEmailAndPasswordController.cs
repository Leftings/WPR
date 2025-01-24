using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using WPR.Controllers.vehicleManager.ChangeUserEmailAndPasswordController;
using WPR.Repository;

namespace WPR.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChangeUserCredentialsController : ControllerBase
    {
        private readonly IUserRepository _userRepository;

        public ChangeUserCredentialsController(IUserRepository userRepository)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        }

        /// <summary>
        /// Updates the email and password for a user under a specific business code.
        /// </summary>
        /// <param name="request">Request containing user ID, new email, new password, and business code.</param>
        /// <returns>Response indicating success or failure of the operation.</returns>
        [HttpPost("user")]
        public async Task<IActionResult> UpdateUserCredentials([FromBody] UpdateUserCredentialsRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.NewEmail) ||
                string.IsNullOrWhiteSpace(request.NewPassword))
            {
                return BadRequest(new { message = "Invalid request data" });
            }

            try
            {
                var result = await _userRepository.UpdateUserEmailAndPasswordAsync(
                    request.UserId,
                    request.NewEmail,
                    request.NewPassword,
                    request.BusinessCode
                );

                if (result.StatusCode == 200)
                {
                    return Ok(new { message = result.Message });
                }

                return BadRequest(new { message = result.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500,
                    new { message = "An error occurred while updating user credentials", error = ex.Message });
            }
        }

        /// <summary>
        /// Updates the email and password for a vehicle manager under a specific business code.
        /// </summary>
        /// <param name="request">Request containing user ID, new email, new password, and business code.</param>
        /// <returns>Response indicating success or failure of the operation.</returns>
        [HttpPost("vehicle-manager")]
        public async Task<IActionResult> UpdateVehicleManagerCredentials([FromBody] UpdateUserCredentialsRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.NewEmail) ||
                string.IsNullOrWhiteSpace(request.NewPassword))
            {
                return BadRequest(new { message = "Invalid request data" });
            }

            try
            {
                var result = await _userRepository.UpdateUserEmailAndPasswordAsync(
                    request.UserId,
                    request.NewEmail,
                    request.NewPassword,
                    request.BusinessCode
                );

                if (result.StatusCode == 200)
                {
                    return Ok(new { message = result.Message });
                }

                return BadRequest(new { message = result.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500,
                    new { message = "An error occurred while updating vehicle manager credentials", error = ex.Message });
            }
        }
    }
}
