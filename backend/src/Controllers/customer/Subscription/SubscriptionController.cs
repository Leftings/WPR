using Microsoft.AspNetCore.Mvc;
using WPR.Repository;


namespace WPR.Controllers.customer.Subscription;

[Route("api/[controller]")]
[ApiController]
public class SubscriptionController : ControllerBase
{
    private readonly IUserRepository _userRepository;

    public SubscriptionController(IUserRepository userRepository)
    {
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
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
}