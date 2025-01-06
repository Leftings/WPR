namespace Employee.Controllers.AddBusiness;

using Microsoft.AspNetCore.Mvc;
using Employee.Repository;

[Route("api/[controller]")]
[ApiController]
public class AddBusinessController : ControllerBase
{
    private readonly IUserRepository _userRepository;

    public AddBusinessController (IUserRepository userRepository)
    {
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
    }

    [HttpPost("addBusiness")]
    public async Task<IActionResult> AddBusiness([FromBody] AddBusinessRequest request)
    {
        var output = _userRepository.AddBusiness(request);
        if (output.status)
        {
            return Ok(new { output.message });
        }
        return BadRequest(new { output.message });
    }
}