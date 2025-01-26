namespace WPR.Controllers.customer.Subscription;

public class SubscriptionRequest
{
    public int? Id { get; set; }
    public string Type { get; set; }
    public string Description { get; set; }
    public double Discount { get; set; }
    public double Price { get; set; }

}
