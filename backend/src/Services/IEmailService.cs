namespace WPR.Services;

public interface IEmailService
{
    Task SendBusinessReviewEmail(string toEmail, string businessName, string domain, string password, bool accepted);

    Task SendConfirmationEmailBusiness(string toEmail, string subscription, string businessName, int kvk, string domain,
        string adres);

    Task SendRentalConfirmMail(string toEmail, string carName, string carColor, string carPlate, DateTime startDate,
        DateTime endDate, string price);

    Task SendWelcomeEmail(string toEmail);
    Task Send(string toEmail, string subject, string body);
    
}