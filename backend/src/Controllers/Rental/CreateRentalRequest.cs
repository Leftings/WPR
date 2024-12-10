namespace WPR.Controllers.Rental
{
    public class CreateRentalRequest
    {
        public string Email { get; set; }  // Used to find the existing customer by email
        public int FrameNrCar { get; set; } // The vehicle the customer wants to rent
        public DateTime StartDate { get; set; } // Rental start date
        public DateTime EndDate { get; set; } // Rental end date
        public decimal Price { get; set; } // Rental price
    }

}
