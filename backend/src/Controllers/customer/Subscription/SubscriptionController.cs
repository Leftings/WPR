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
            Console.WriteLine(e);
            throw;
        }
    }

    [HttpGet("GetSubscriptionData")]
    public async Task<IActionResult> GetSubscriptionData(int id)
    {
        try
        {
            var data = await _userRepository.GetSubscriptionDataAsync(id);

            if (data == null)
            {
                NotFound("No subscription data found");
            }
            
            return Ok(new { message = data });
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    [HttpGet("GetSubscriptionIds")]
    public async Task<IActionResult> GetSubscriptionIdsAsync()
    {
        try
        {
            var ids = await _userRepository.GetSubscriptionIdsAsync();

            if (ids == null)
            {
                NotFound("Could not find subscription ids");
            }

            return Ok(new {message = ids});
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    [HttpPost("AddSubscription")]
    public async Task<IActionResult> AddSubscription([FromBody] Subscription subscription)
    {
        try
        {
            var status = await _backOfficeRepository.AddSubscriptionAsync(
                subscription.Type,
                subscription.Description,
                subscription.Discount);

            if (status.status)
            {
                return Ok( new { status.message });
            }
            
            return BadRequest( new { status.message });
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
    
    
    

    [HttpDelete("DeleteSubscription")]
    public async Task<IActionResult> DeleteSubscriptionAsync(int id)
    {
        if (id <= 0)
        {
            return BadRequest(new { status = false, message = "Invalid subscription ID"});
        }
        
        try
        {
            var result = await _backOfficeRepository.DeleteSubscriptionAsync(id);

            if (result.status)
            {
                return Ok(new { status = true, result.message });
            }
            return BadRequest(new { status = false, result.message });
            
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}