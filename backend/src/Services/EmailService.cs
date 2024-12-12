using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;
using WPR.Data;

namespace WPR.Services;

public class EmailService
{
    private readonly EnvConfig _envConfig; // Instantie van EnConfig om environment variabelen op te halen.

    public EmailService(EnvConfig envConfig)
    {
        _envConfig = envConfig ?? throw new ArgumentNullException(nameof(envConfig));
    }

    private SmtpClient CreateSmtpClient()
    {
        return new SmtpClient(_envConfig.Get("SMTP_HOST"))
        {
            Port = int.Parse(_envConfig.Get("SMTP_PORT")),
            Credentials = new NetworkCredential(
                _envConfig.Get("SMTP_FROM_EMAIL"),
                _envConfig.Get("SMTP_FROM_PASSWORD")
                ),
            EnableSsl = true
        };
    }
    
    public async Task SendWelcomeEmail(string toEmail)
    {     
        if (string.IsNullOrEmpty(toEmail))
        {
            throw new ArgumentException("Email addresses cannot be null or empty");
        }
        
        try
        {
            using var smtpClient = CreateSmtpClient();
            using var mailMessage = new MailMessage
            {
                From = new MailAddress(_envConfig.Get("SMTP_FROM_EMAIL")),
                Subject = "Welkom bij CarAndAll",
                Body = $"Welkom bij CarAndAll",
                IsBodyHtml = true
            };
            mailMessage.To.Add(toEmail);

            await smtpClient.SendMailAsync(mailMessage);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
    
    public async Task SendRentalConfirmMail(string toEmail, string carName, string carColor, string carPlate , DateTime startDate, DateTime endDate, string price)
    {
        if (string.IsNullOrEmpty(toEmail))
        {
            throw new ArgumentException("Email addresses cannot be null or empty");
        }
        
        try
        {
            using var smtpClient = CreateSmtpClient();
            using var mailMessage = new MailMessage
            {
                From = new MailAddress(_envConfig.Get("SMTP_FROM_EMAIL")),
                Subject = "Verificatie Voertuig Huren",
                Body = BuildRentalConfirmationBody(carName, carColor, carPlate, startDate, endDate, price),
                IsBodyHtml = true
            };
            mailMessage.To.Add(toEmail);

            await smtpClient.SendMailAsync(mailMessage);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
    
    private string BuildRentalConfirmationBody(string carName, string carColor, string carPlate, DateTime startDate, DateTime endDate, string price)
    {
        return $@"
            <p>Bedankt voor het huren van een voeruig bij CarAndAll,</p>
            <p><strong>Uw huur auto:</strong></p>
            <ul>
                <li>Voertuig naam: {System.Net.WebUtility.HtmlEncode(carName)}</li>
                <li>Voertuig kleur: {System.Net.WebUtility.HtmlEncode(carColor)}</li>
                <li>Voertuig kenteken: {System.Net.WebUtility.HtmlEncode(carPlate)}</li>
            </ul>
            <p><strong>Uw huur periode:</strong></p>
            <ul>
                <li>Vanaf: {startDate:MMMM dd, yyyy}</li>
                <li>Tot en met: {endDate:MMMM dd, yyyy}</li>
            </ul>
            <p><strong>Totale huur prijs:</strong> €{System.Net.WebUtility.HtmlEncode(price)}</p>
        ";
    }
}
