using Microsoft.AspNetCore.Mvc;
using WPR.Repository;

namespace WPR.Controllers.customer.Subscription;

[Route("api/[controller]")]
[ApiController]
public class SubscriptionController : ControllerBase
{
    private readonly IUserRepository _userRepository;
    private readonly IBackOfficeRepository _backOfficeRepository;

    // Constructor ontvangt de repositories voor gebruikers en backoffice operaties.
    public SubscriptionController(IUserRepository userRepository, IBackOfficeRepository backOfficeRepository)
    {
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _backOfficeRepository = backOfficeRepository ?? throw new ArgumentNullException(nameof(backOfficeRepository));
    }

    /// <summary>
    /// Haalt alle abonnementen van de gebruiker op.
    /// </summary>
    /// <returns>Een lijst van abonnementen of een foutmelding.</returns>
    [HttpGet("GetSubscriptions")]
    public async Task<IActionResult> GetSubscriptions()
    {
        try
        {
            // Haal alle abonnementen van de gebruiker op.
            var subscription = await _userRepository.GetAllSubscriptionsAsync();
            
            // Controleer of er geen abonnementen zijn gevonden.
            if (subscription == null || !subscription.Any())
            {
                return NotFound("No subscriptions found");
            }

            // Retourneer de gevonden abonnementen.
            return Ok(new { data = subscription });

        }
        catch (Exception e)
        {
            // Log de fout en hergooi deze.
            Console.WriteLine(e);
            throw;
        }
    }

    /// <summary>
    /// Haalt specifieke gegevens op van een abonnement op basis van het opgegeven ID.
    /// </summary>
    /// <param name="id">Het ID van het abonnement waarvoor gegevens opgehaald moeten worden.</param>
    /// <returns>De gegevens van het abonnement of een foutmelding.</returns>
    [HttpGet("GetSubscriptionData")]
    public async Task<IActionResult> GetSubscriptionData(int id)
    {
        try
        {
            // Haal de gegevens op van het opgegeven abonnement.
            var data = await _userRepository.GetSubscriptionDataAsync(id);

            // Als er geen gegevens zijn gevonden, retourneer een foutmelding.
            if (data == null)
            {
                return NotFound("No subscription data found");
            }
            
            // Retourneer de abonnementgegevens.
            return Ok(new { message = data });
        }
        catch (Exception e)
        {
            // Log de fout en hergooi deze.
            Console.WriteLine(e);
            throw;
        }
    }

    /// <summary>
    /// Haalt een lijst van alle abonnement-ID's op.
    /// </summary>
    /// <returns>Een lijst van abonnement-ID's of een foutmelding.</returns>
    [HttpGet("GetSubscriptionIds")]
    public async Task<IActionResult> GetSubscriptionIdsAsync()
    {
        try
        {
            // Haal de abonnement-ID's op.
            var ids = await _userRepository.GetSubscriptionIdsAsync();

            // Als er geen IDs zijn gevonden, retourneer een foutmelding.
            if (ids == null)
            {
                return NotFound("Could not find subscription ids");
            }

            // Retourneer de lijst met abonnement-ID's.
            return Ok(new { message = ids });
        }
        catch (Exception e)
        {
            // Log de fout en hergooi deze.
            Console.WriteLine(e);
            throw;
        }
    }

    /// <summary>
    /// Voegt een nieuw abonnement toe aan het systeem.
    /// </summary>
    /// <param name="subscription">De gegevens van het abonnement dat toegevoegd moet worden.</param>
    /// <returns>Een succes- of foutmelding met de status van het toevoegen van het abonnement.</returns>
    [HttpPost("AddSubscription")]
    public async Task<IActionResult> AddSubscription([FromBody] Subscription subscription)
    {
        try
        {
            // Voeg het abonnement toe via de backoffice repository.
            var status = await _backOfficeRepository.AddSubscriptionAsync(
                subscription.Type,
                subscription.Description,
                subscription.Discount,
                subscription.Price);
            
            // Als het abonnement succesvol is toegevoegd, stuur dan een succesmelding.
            if (status.status)
            {
                return Ok(new { status.message });
            }
            
            // Als het toevoegen van het abonnement mislukt, stuur dan een foutmelding.
            return BadRequest(new { status.message });
        }
        catch (Exception e)
        {
            // Log de fout en hergooi deze.
            Console.WriteLine(e);
            throw;
        }
    }

    /// <summary>
    /// Verwijdert een abonnement uit het systeem op basis van het gegeven abonnement-ID.
    /// </summary>
    /// <param name="id">Het ID van het abonnement dat verwijderd moet worden.</param>
    /// <returns>Een succes- of foutmelding met de status van het verwijderen van het abonnement.</returns>
    [HttpDelete("DeleteSubscription")]
    public async Task<IActionResult> DeleteSubscriptionAsync(int id)
    {
        // Controleer of het ID geldig is.
        if (id <= 0)
        {
            return BadRequest(new { status = false, message = "Invalid subscription ID" });
        }
        
        try
        {
            // Verwijder het abonnement via de backoffice repository.
            var result = await _backOfficeRepository.DeleteSubscriptionAsync(id);

            // Als het verwijderen van het abonnement succesvol is, stuur dan een succesmelding.
            if (result.status)
            {
                return Ok(new { status = true, result.message });
            }

            // Als het verwijderen van het abonnement mislukt, stuur dan een foutmelding.
            return BadRequest(new { status = false, result.message });
        }
        catch (Exception e)
        {
            // Log de fout en hergooi deze.
            Console.WriteLine(e);
            throw;
        }
    }
}
