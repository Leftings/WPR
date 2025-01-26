using WPR.Repository;
using Microsoft.AspNetCore.Mvc;

namespace WPR.Controllers.Employee.Shared.viewRentalData; 

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
    public async Task<IActionResult> GetReview(int id)
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
            return Ok(new { message = response.Ids});
        }
        return BadRequest(new { message = response.Message});
    }
    
    [HttpGet("GetFullReviewData")]
    public async Task<IActionResult> GetFullReview(int id)
    {
        try
        {
            (bool Status, string Message, Dictionary<string, object> Data) response = _backOfficeRepository.GetFullDataReview(id);

            if (response.Status)
            {
                return Ok(new viewRentalDataResponse{ Message = response.Data });
            }
            return BadRequest(new viewRentalDataErrorResponse() { Status = false, Message = response.Message });
        }
        catch (Exception e)
        {
            return BadRequest(new viewRentalDataErrorResponse
            {
                Status = false,
                Message = e.Message
            });
        }
    }
}