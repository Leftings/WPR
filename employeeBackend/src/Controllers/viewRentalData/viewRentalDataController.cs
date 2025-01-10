using Employee.Repository;
using Microsoft.AspNetCore.Mvc;

namespace Employee.Controllers.viewRentalData; 

[ApiController]
[Route("api/[controller]")]
public class viewRentalDataController : ControllerBase
{
    private readonly IBackOfficeRepository _backOfficeRepository;
    public viewRentalDataController(IBackOfficeRepository backOfficeRepository)
    {
        _backOfficeRepository = backOfficeRepository ?? throw new ArgumentNullException(nameof(backOfficeRepository));
    }

    [HttpGet("/GetReviews")]
    public async Task<IActionResult> GetReviews(string sort, string how)
    {
        (bool Status, string Message, IList<Dictionary<string, object>> Data) response = _backOfficeRepository.GetDataReviews(sort, how);

        if (response.Status)
        {
            return Ok(new { message = response.Data });
        }
        return BadRequest(new { message = response.Message });
    }
}