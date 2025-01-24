using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using WPR.Repository;

namespace WPR.Controllers.vehicleManager.ChangeUserEmailAndPasswordController
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChangeUserEmailAndPasswordController : ControllerBase
    {
        private readonly IUserRepository _userRepository;

        public ChangeUserEmailAndPasswordController(IUserRepository userRepository)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        }

        /// <summary>
        /// Updates the email and password for a user under a specific business code.
        /// </summary>
        /// <param name="request">Request containing user ID, new email, and new password.</param>
        /// <returns>Response indicating success or failure of the operation.</returns>
        [HttpPost("updateUserCredentials")]
        public async Task<IActionResult> UpdateUserCredentials([FromBody] UpdateUserCredentialsRequest request)
        {
            if (request == null)
            {
                return BadRequest(new { message = "Invalid request data" });
            }

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
    }
}