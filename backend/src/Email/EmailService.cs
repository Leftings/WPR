using MailKit.Net.Smtp;
using MimeKit;
using System.Threading.Tasks;

namespace WPR.Services
{
    public interface IEmailService
    {
        Task SendConfirmationEmailAsync(string toEmail, string name, string rentalDates, decimal totalCost);
    }

    public class EmailService : IEmailService
    {
        private readonly string smtpServer = "smtp.gmail.com"; // Replace with your SMTP server
        private readonly int smtpPort = 587; // Common SMTP port
        private readonly string smtpUsername = "carandallmailservice@gmail.com"; // SMTP username
        private readonly string smtpPassword = "Wonderfulkoe1"; // SMTP password

        public async Task SendConfirmationEmailAsync(string toEmail, string name, string rentalDates, decimal totalCost)
        {
            var emailMessage = new MimeMessage();
            emailMessage.From.Add(new MailboxAddress("Car Rental Service", smtpUsername));
            emailMessage.To.Add(new MailboxAddress(name, toEmail));
            emailMessage.Subject = "Rental Confirmation";

            var body = $"Dear {name},\n\n" +
                       $"Thank you for renting with us! Your rental has been successfully confirmed.\n\n" +
                       $"Rental Period: {rentalDates}\n" +
                       $"Total Cost: €{totalCost:F2}\n\n" +
                       "We look forward to serving you!\n\n" +
                       "Best regards,\n" +
                       "Car Rental Service Team";

            emailMessage.Body = new TextPart("plain")
            {
                Text = body
            };

            using (var client = new SmtpClient())
            {
                await client.ConnectAsync(smtpServer, smtpPort, false);
                await client.AuthenticateAsync(smtpUsername, smtpPassword);
                await client.SendAsync(emailMessage);
                await client.DisconnectAsync(true);
            }
        }
    }
}
