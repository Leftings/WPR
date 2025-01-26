namespace WPR.Controllers.Customer.Rental;

/// <summary>
/// Klasse die de gegevens bevat voor een huurverzoek van een klant.
/// </summary>
public class RentalRequest
{
    public string Email { get; set; }       // Customer's email
    public string FrameNrVehicle { get; set; } // Vehicle Frame Number
    public DateTime StartDate { get; set; } // Rental start date
    public DateTime EndDate { get; set; }   // Rental end date
    public decimal Price { get; set; }      // Rental price
}

