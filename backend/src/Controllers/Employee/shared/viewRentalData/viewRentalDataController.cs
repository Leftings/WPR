using WPR.Repository;
using Microsoft.AspNetCore.Mvc;

namespace WPR.Controllers.Employee.Shared.viewRentalData;

[ApiController]
[Route("api/[controller]")]
public class viewRentalDataController : ControllerBase
{
    private readonly IBackOfficeRepository _backOfficeRepository;

    // Constructor die de repository injecteert, verantwoordelijk voor de communicatie met de backend
    public viewRentalDataController(IBackOfficeRepository backOfficeRepository)
    {
        _backOfficeRepository = backOfficeRepository ?? throw new ArgumentNullException(nameof(backOfficeRepository));
    }

    /// <summary>
    /// Haalt de review data op voor een specifiek ID.
    /// </summary>
    /// <param name="id">Het ID van de review waarvoor data opgehaald moet worden</param>
    /// <returns>Als het succesvol is, wordt de data teruggegeven, anders een foutmelding.</returns>
    [HttpGet("GetReviewData")]
    public async Task<IActionResult> GetReview(int id)
    {
        // Haal de review data op uit de repository op basis van het ID
        (bool Status, string Message, Dictionary<string, object> Data) response = _backOfficeRepository.GetDataReview(id);

        // Controleer of de status van de response 'true' is
        if (response.Status)
        {
            // Als de status 'true' is, retourneer de data in de response
            return Ok(new { message = response.Data });
        }
        
        // Als de status 'false' is, retourneer een foutmelding
        return BadRequest(new { message = response.Message });
    }

    /// <summary>
    /// Haalt de review ID's op.
    /// </summary>
    /// <returns>De lijst van review ID's als het succesvol is, anders een foutmelding.</returns>
    [HttpGet("GetReviewsIds")]
    public async Task<IActionResult> GetDataReviewIds()
    {
        // Haal de review ID's op uit de repository
        (bool Status, string Message, int[] Ids) response = _backOfficeRepository.GetDataReviewIds();

        // Als de status succesvol is, geef dan de ID's terug
        if (response.Status)
        {
            return Ok(new { message = response.Ids});
        }
        return BadRequest(new { message = response.Message});
    }

    /// <summary>
    /// Haalt gedetailleerde review data op voor een specifiek ID.
    /// </summary>
    /// <param name="id">Het ID van de review waarvoor gedetailleerde data opgehaald moet worden</param>
    /// <returns>De gedetailleerde review data als het succesvol is, anders een foutmelding.</returns>
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