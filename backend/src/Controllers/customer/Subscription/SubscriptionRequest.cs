namespace WPR.Controllers.customer.Subscription;

/// <summary>
/// Model voor het aanvragen van een nieuw abonnement of het bijwerken van een bestaand abonnement.
/// </summary>
public class SubscriptionRequest
{
    public int? Id { get; set; }
    public string Type { get; set; }
    public string Description { get; set; }
    public double Discount { get; set; }
    public double Price { get; set; }

}
