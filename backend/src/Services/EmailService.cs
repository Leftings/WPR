using System.Net;
using System.Net.Mail;
using System.Reflection.Metadata.Ecma335;
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

    /// <summary>
    /// Maak SMTP Client voor versturen emails
    /// </summary>
    /// <returns>SMTP Client</returns>
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

    public async Task SendConfirmationEmailBusiness(string toEmail, string businessName, int kvk, string domain, string adres)
    {
        if (string.IsNullOrEmpty(toEmail))
        {
            throw new ArgumentNullException("Email can not be null");
        }

        try
        {
            using (var smtpClient = CreateSmtpClient())
            using (var emailMessage = new MailMessage
                {
                    From = new MailAddress(_envConfig.Get("SMTP_FROM_EMAIL")),
                    Subject = "Aanvraag Bedrijfs Account",
                    Body = BuildConfirmationEmailBusinessBody(businessName, kvk, domain, adres),
                    IsBodyHtml = true

                })
            {
                emailMessage.To.Add(toEmail);
                
                await smtpClient.SendMailAsync(emailMessage);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            throw;
        }
    }
    
    public async Task SendBusinessReviewEmail(string toEmail, string businessName, string domain, string password, bool accepted)
    {
        if (string.IsNullOrEmpty(toEmail))
        {
            throw new ArgumentNullException("Email can not be null");
        }

        try
        {
            using (var smtpClient = CreateSmtpClient())
            using (var emailMessage = new MailMessage
                {
                    From = new MailAddress(_envConfig.Get("SMTP_FROM_EMAIL")),
                    Subject = "Aanvraag Bedrijfs Account: Review",
                    Body = BuildBusinessReviewEmailBody(businessName, domain, password, accepted),
                    IsBodyHtml = true

                })
            {
                emailMessage.To.Add(toEmail);
                
                await smtpClient.SendMailAsync(emailMessage);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            throw;
        }
    }
    private string BuildRentalConfirmationBody(string carName, string carColor, string carPlate, DateTime startDate, DateTime endDate, string price)
    {
        return $@"
            <p>Bedankt voor het huren van een voertuig bij CarAndAll,</p>
            <p>U krijgt binnen 48 uur bericht of uw aanvraag is geaccepteerd of geweigerd,</p>

            <p><strong>Uw huur auto:</strong></p>
            <ul>
                <li>Voertuig naam: {WebUtility.HtmlEncode(carName)}</li>
                <li>Voertuig kleur: {WebUtility.HtmlEncode(carColor)}</li>
                <li>Voertuig kenteken: {WebUtility.HtmlEncode(carPlate)}</li>
            </ul>
            <p><strong>Uw huur periode:</strong></p>
            <ul>
                <li>Vanaf: {startDate:dd MMMM, yyyy}</li>
                <li>Tot en met: {endDate:dd MMMM, yyyy}</li>
            </ul>
            <p><strong>Totale huur prijs:</strong> €{WebUtility.HtmlEncode(price)}</p>
        ";
    }

    private string BuildConfirmationEmailBusinessBody(string businessName, int kvk, string domain, string adres)
    {
        return $@"
            <h1>Welkom, {businessName}</h1>

            <p>Bedankt voor het aanmelden als bedrijf bij CarAndAll.</p>
            <p>Ons personeel zal binnen 48 uw aanvraag voor een bedrijfsaccount bekijken en verwerken met de volgende gegevens:</p>
            <ul>
                <li>Bedrijfsnaam: {businessName}</li>
                <li>Kamer van koophandel: {kvk}</li>
                <li>Domeinnaam: {domain}
                <li>Adres Hoofdkantoor: {adres}</li>
            </ul>
            
            <p>Dit is een automatisch gegeneerd bericht van CarAndAll.</p>
            <p>Reacties op dit bericht worden niet gezien.</p>";
    }

    private string BuildBusinessReviewEmailBody(string businessName, string domain, string password, bool accepted)
    {
        string response = accepted ? 
        $@"<h1>{businessName}</h1>

        <p>Gefeliciteerd met het succesvol verkrijgen van een zakelijkaccount bij CarAndAll.<p>
        <p>Alle huuraanvragen van emailaddressen die eindigen met {domain} worden geplaats voor review bij uw wagenparkbeheerd.</p>
        <p>De inlog voor uw wagenparkbeheerder is: 
        <ul>
            <li>Gebruikersnaam: wagenparkbeheerder{domain}</li>
            <li>Wachtwoord: {password}
        </ul>
        <p>Dit is een automatisch gegeneerd bericht van CarAndAll.</p>
        <p>Reacties op dit bericht worden niet gezien.</p>"
        :
        $@"<h1>{businessName}</h1>
        <p>Helaas voldoet uw bedrijf niet tot de toelatingseisen die gesteled worden bij CarAndAll</p>
        <p>Mocht u denken dat dit een fout is, doe dan wederom nog een aanvraag voor een zakelijk account voor uw bedrijf.
        
        <p>Dit is een automatisch gegeneerd bericht van CarAndAll.</p>
        <p>Reacties op dit bericht worden niet gezien.</p>";
        
        return response;
    }
}
