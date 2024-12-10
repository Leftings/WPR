using MailKit.Net.Smtp;
using MimeKit;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;

namespace WPR.Services
{
    public class EmailService : IEmailService
    {
        private readonly EmailSettings _emailSettings;

        public EmailService(IOptions<EmailSettings> emailSettings)
        {
            _emailSettings = emailSettings.Value ?? throw new ArgumentNullException(nameof(emailSettings));
        }

        public async Task SendEmailAsync(string to, string subject, string body)
        {
            if (string.IsNullOrEmpty(to))
            {
                throw new ArgumentNullException(nameof(to), "Email address cannot be null or empty.");
            }

            var emailMessage = new MimeMessage();
            emailMessage.From.Add(new MailboxAddress("Car Rental Service", _emailSettings.Username));
            emailMessage.To.Add(new MailboxAddress(to, to));
            emailMessage.Subject = subject;

            emailMessage.Body = new TextPart("plain")
            {
                Text = body
            };

            using (var client = new SmtpClient())
            {
                await client.ConnectAsync(_emailSettings.Server, _emailSettings.Port, false);
                await client.AuthenticateAsync(_emailSettings.Username, _emailSettings.Password);
                await client.SendAsync(emailMessage);
                await client.DisconnectAsync(true);
            }
        }

        public async Task SendConfirmationEmailAsync(string toEmail, string name, string rentalDates, decimal totalCost)
        {
            if (string.IsNullOrEmpty(toEmail))
            {
                throw new ArgumentNullException(nameof(toEmail), "Email address cannot be null or empty.");
            }

            var subject = "Rental Confirmation";
            var body = $"Dear {name},\n\n" +
                       $"Thank you for renting with us! Your rental has been successfully confirmed.\n\n" +
                       $"Rental Period: {rentalDates}\n" +
                       $"Total Cost: €{totalCost:F2}\n\n" +
                       "We look forward to serving you!\n\n" +
                       "Best regards,\n" +
                       "Car Rental Service Team";

            await SendEmailAsync(toEmail, subject, body);
        }
    }
}
