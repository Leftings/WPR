namespace Employee.Controllers.AddBusiness;

using Microsoft.AspNetCore.Mvc;
using Employee.Repository;

/// <summary>
/// Controller voor het verbinden van het aanmaken van bedrijven op basis van het KVK nummer
/// </summary>
[Route("api/[controller]")]
[ApiController]
public class AddBusinessController : ControllerBase
{
    private readonly IUserRepository _userRepository;

    public AddBusinessController (IUserRepository userRepository)
    {
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
    }

    /// <summary>
    /// Maakt een connectie met UserRepository en geeft het antwoord terug
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost("addBusiness")]
    public async Task<IActionResult> AddBusiness([FromForm] AddBusinessRequest request)
    {
        var output = _userRepository.AddBusiness(request);
        if (output.status)
        {
            return Ok(new { output.message });
        }
        return BadRequest(new { output.message });
    }
}