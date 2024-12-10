namespace WPR.Services
{
    public interface IEmailService
    {
        Task SendEmailAsync(string to, string subject, string body);
        Task SendConfirmationEmailAsync(string toEmail, string name, string rentalDates, decimal totalCost);
    }
}