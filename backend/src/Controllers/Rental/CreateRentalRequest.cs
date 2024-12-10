namespace WPR.Controllers.Rental
{
    public class RentalRequest
    {
        public string Email { get; set; }       // Customer's email
        public string FrameNrCar { get; set; } // Vehicle Frame Number
        public DateTime StartDate { get; set; } // Rental start date
        public DateTime EndDate { get; set; }   // Rental end date
        public decimal Price { get; set; }      // Rental price
    }
}
