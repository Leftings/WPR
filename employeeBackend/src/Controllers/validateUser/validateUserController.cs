using Microsoft.AspNetCore.Mvc;

namespace Employee.Controllers.validateUser;

[Route("api/[controller]")]
[ApiController]
public class ValidateUserController : ControllerBase
{
    [HttpGet("CheckSessionStaff")]
    public IActionResult CheckSessionStaff()
    {
        string sessionValue = Request.Cookies["LoginEmployeeSession"];
        
        if (!string.IsNullOrEmpty(sessionValue))
        {
            return Ok( new {message = "session active ", sessionValue});
        }
        return Unauthorized(new { message = "Session expired or is not found" });
    }
}