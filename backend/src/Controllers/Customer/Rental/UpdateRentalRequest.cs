namespace WPR.Controllers.Customer.Rental;

public class UpdateRentalRequest
{
    public int Id { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal Price { get; set; }
}