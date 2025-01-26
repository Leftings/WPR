using Microsoft.AspNetCore.Mvc;
using WPR.Repository;

namespace WPR.Controllers.customer.Subscription;

[Route("api/[controller]")]
[ApiController]
public class SubscriptionController : ControllerBase
{
    private readonly IUserRepository _userRepository;
    private readonly IBackOfficeRepository _backOfficeRepository;

    public SubscriptionController(IUserRepository userRepository, IBackOfficeRepository backOfficeRepository)
    {
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _backOfficeRepository = backOfficeRepository ?? throw new ArgumentNullException(nameof(backOfficeRepository));
    }

    /// <summary>
    /// Haalt alle abonnementen op.
    /// </summary>
    /// <returns>Een lijst van alle abonnementen.</returns>
    [HttpGet("GetSubscriptions")]
    public async Task<IActionResult> GetSubscriptions()
    {
        try
        {
            var subscription = await _userRepository.GetAllSubscriptionsAsync();
            
            if (subscription == null || !subscription.Any())
            {
                return NotFound("No subscriptions found");
            }

            return Ok(new { data = subscription });

        }
        catch (Exception e)
        {
            return StatusCode(500, new SubscriptionErrorResponse
            {
                Status = false,
                Message = e.Message
            });
        }
    }

    /// <summary>
    /// Haalt specifieke gegevens van een abonnement op.
    /// </summary>
    /// <param name="id">Het ID van het abonnement.</param>
    /// <returns>De gegevens van het abonnement met het opgegeven ID.</returns>
    [HttpGet("GetSubscriptionData")]
    public async Task<IActionResult> GetSubscriptionData(int id)
    {
        try
        {
            var data = await _userRepository.GetSubscriptionDataAsync(id);

            if (data == null)
            {
                return NotFound(new SubscriptionErrorResponse
                {
                    Status = false,
                    Message = "No subscription data found"
                });
            }
            
            return Ok(new { Message = data });
        }
        catch (Exception e)
        {
            return StatusCode(500, new SubscriptionErrorResponse
            {
                Status = false,
                Message = e.Message
            });
        }
    }

    /// <summary>
    /// Haalt alle abonnement-IDs op.
    /// </summary>
    /// <returns>Een lijst van alle abonnement-IDs.</returns>
    [HttpGet("GetSubscriptionIds")]
    public async Task<IActionResult> GetSubscriptionIdsAsync()
    {
        try
        {
            var ids = await _userRepository.GetSubscriptionIdsAsync();

            if (ids == null)
            {
                return NotFound(new SubscriptionErrorResponse
                {
                    Status = false,
                    Message = "Could not find subscription ids"
                });
            }

            return Ok(new { Message = ids });
        }
        catch (Exception e)
        {
            return StatusCode(500, new SubscriptionErrorResponse
            {
                Status = false,
                Message = e.Message
            });
        }
    }

    /// <summary>
    /// Voegt een nieuw abonnement toe.
    /// </summary>
    /// <param name="subscriptionRequest">De gegevens van het abonnement dat toegevoegd moet worden.</param>
    /// <returns>Een succes- of foutmelding met betrekking tot de toevoeging van het abonnement.</returns>
    [HttpPost("AddSubscription")]
    public async Task<IActionResult> AddSubscription([FromBody] SubscriptionRequest subscriptionRequest)
    {
        try
        {
            var status = await _backOfficeRepository.AddSubscriptionAsync(
                subscriptionRequest.Type,
                subscriptionRequest.Description,
                subscriptionRequest.Discount,
                subscriptionRequest.Price);
            
            if (status.status)
            {
                return Ok(new SubscriptionResponse { Message = status.message });
            }
            
            return BadRequest(new SubscriptionErrorResponse
            {
                Status = false,
                Message = status.message
            });
        }
        catch (Exception e)
        {
            return StatusCode(500, new SubscriptionErrorResponse
            {
                Status = false,
                Message = e.Message
            });
        }
    }

    /// <summary>
    /// Verwijdert een specifiek abonnement op basis van het ID.
    /// </summary>
    /// <param name="id">Het ID van het abonnement dat verwijderd moet worden.</param>
    /// <returns>Een succes- of foutmelding met betrekking tot de verwijdering van het abonnement.</returns>
    [HttpDelete("DeleteSubscription")]
    public async Task<IActionResult> DeleteSubscriptionAsync(int id)
    {
        try
        {
            var result = await _backOfficeRepository.DeleteSubscriptionAsync(id);

            if (result.status)
            {
                return Ok(new SubscriptionResponse { Message = result.message });
            }
            return BadRequest(new SubscriptionErrorResponse
            {
                Status = false,
                Message = result.message
            });
            
        }
        catch (Exception e)
        {
            return StatusCode(500, new SubscriptionErrorResponse
            {
                Status = false,
                Message = e.Message
            });
        }
    }
}
