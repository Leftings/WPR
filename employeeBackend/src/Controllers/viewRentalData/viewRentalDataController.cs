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

    [HttpGet("GetReviewData")]
    public async Task<IActionResult> GetReviews(int id)
    {
        (bool Status, string Message, Dictionary<string, object> Data) response = _backOfficeRepository.GetDataReview(id);

        if (response.Status)
        {
            return Ok(new { message = response.Data });
        }
        return BadRequest(new { message = response.Message });
    }

    [HttpGet("GetReviewsIds")]
    public async Task<IActionResult> GetDataReviewIds()
    {
        (bool Status, string Message, int[] Ids) response = _backOfficeRepository.GetDataReviewIds();

        if (response.Status)
        {
            Console.WriteLine("OK");
            return Ok(new { message = response.Ids});
        }
        return BadRequest(new { message = response.Message});
    }
}