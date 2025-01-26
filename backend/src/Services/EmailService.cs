using System.Net;
using System.Net.Mail;
using WPR.Data;

namespace WPR.Services;

public class EmailService : IEmailService
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

    // Methode om een e-mail te versturen
public async Task Send(string toEmail, string subject, string body)
{
    try
    {
        using (var smtpClient = CreateSmtpClient())
        using (var mailMessage = new MailMessage
        {
            From = new MailAddress(_envConfig.Get("SMTP_FROM_EMAIL")), // Afzender e-mail
            Subject = subject, // Onderwerp van de e-mail
            Body = body, // Inhoud van de e-mail
            IsBodyHtml = true // Geef aan dat de inhoud HTML is
        })
        {
            mailMessage.To.Add(toEmail); // Voeg de ontvanger toe
            await smtpClient.SendMailAsync(mailMessage); // Verstuur de e-mail asynchroon
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine(ex.Message); // Log eventuele fouten
        throw; // Gooi de uitzondering opnieuw
    }
}

// Methode om een welkomst e-mail te versturen
public async Task SendWelcomeEmail(string toEmail)
{     
    if (string.IsNullOrEmpty(toEmail))
    {
        throw new ArgumentException("Email addresses cannot be null or empty"); // Controleer of het e-mailadres geldig is
    }
    
    try
    {
        using var smtpClient = CreateSmtpClient();
        using var mailMessage = new MailMessage
        {
            From = new MailAddress(_envConfig.Get("SMTP_FROM_EMAIL")), // Afzender e-mail
            Subject = "Welkom bij CarAndAll", // Onderwerp van de e-mail
            Body = $"Welkom bij CarAndAll", // Inhoud van de e-mail
            IsBodyHtml = true // Geef aan dat de inhoud HTML is
        };
        mailMessage.To.Add(toEmail); // Voeg de ontvanger toe

        await smtpClient.SendMailAsync(mailMessage); // Verstuur de e-mail asynchroon
    }
    catch (Exception e)
    {
        Console.WriteLine(e); // Log eventuele fouten
        throw; // Gooi de uitzondering opnieuw
    }
}

// Methode om een huurovereenkomst bevestigingsmail te versturen
public async Task SendRentalConfirmMail(string toEmail, string carName, string carColor, string carPlate, DateTime startDate, DateTime endDate, string price)
{
    if (string.IsNullOrEmpty(toEmail))
    {
        throw new ArgumentException("Email addresses cannot be null or empty"); // Controleer of het e-mailadres geldig is
    }

    try
    {
        using var smtpClient = CreateSmtpClient();
        using var mailMessage = new MailMessage
        {
            From = new MailAddress(_envConfig.Get("SMTP_FROM_EMAIL")), // Afzender e-mail
            Subject = "Verificatie Voertuig Huren", // Onderwerp van de e-mail
            Body = BuildRentalConfirmationBody(carName, carColor, carPlate, startDate, endDate, price), // Bouw de inhoud van de bevestiging
            IsBodyHtml = true // Geef aan dat de inhoud HTML is
        };
        mailMessage.To.Add(toEmail); // Voeg de ontvanger toe

        await smtpClient.SendMailAsync(mailMessage); // Verstuur de e-mail asynchroon
    }
    catch (Exception e)
    {
        Console.WriteLine(e); // Log eventuele fouten
        throw; // Gooi de uitzondering opnieuw
    }
}

// Methode om een bevestigingsmail voor een bedrijfsaccount aanvraag te versturen
public async Task SendConfirmationEmailBusiness(string toEmail, string subscription, string businessName, int kvk, string domain, string adres)
{
    if (string.IsNullOrEmpty(toEmail))
    {
        throw new ArgumentNullException("Email can not be null"); // Zorg ervoor dat het e-mailadres geldig is
    }

    try
    {
        using (var smtpClient = CreateSmtpClient())
        using (var emailMessage = new MailMessage
        {
            From = new MailAddress(_envConfig.Get("SMTP_FROM_EMAIL")), // Afzender e-mail
            Subject = "Aanvraag Bedrijfs Account", // Onderwerp van de e-mail
            Body = BuildConfirmationEmailBusinessBody(subscription, businessName, kvk, domain, adres), // Bouw de inhoud van de bevestiging
            IsBodyHtml = true // Geef aan dat de inhoud HTML is
        })
        {
            emailMessage.To.Add(toEmail); // Voeg de ontvanger toe

            await smtpClient.SendMailAsync(emailMessage); // Verstuur de e-mail asynchroon
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine(ex.Message); // Log eventuele fouten
        throw; // Gooi de uitzondering opnieuw
    }
}

// Methode om een beoordelingsmail voor een bedrijfsaccount aanvraag te versturen
public async Task SendBusinessReviewEmail(string toEmail, string businessName, string domain, string password, bool accepted)
{
    if (string.IsNullOrEmpty(toEmail))
    {
        throw new ArgumentNullException("Email can not be null"); // Zorg ervoor dat het e-mailadres geldig is
    }

    try
    {
        using (var smtpClient = CreateSmtpClient())
        using (var emailMessage = new MailMessage
        {
            From = new MailAddress(_envConfig.Get("SMTP_FROM_EMAIL")), // Afzender e-mail
            Subject = "Aanvraag Bedrijfs Account: Review", // Onderwerp van de e-mail
            Body = BuildBusinessReviewEmailBody(businessName, domain, password, accepted), // Bouw de inhoud van de beoordelingsmail
            IsBodyHtml = true // Geef aan dat de inhoud HTML is
        })
        {
            emailMessage.To.Add(toEmail); // Voeg de ontvanger toe

            await smtpClient.SendMailAsync(emailMessage); // Verstuur de e-mail asynchroon
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine(ex.Message); // Log eventuele fouten
        throw; // Gooi de uitzondering opnieuw
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

    private string BuildConfirmationEmailBusinessBody(string subscription ,string businessName, int kvk, string domain, string adres)
    {
        
        return $@"
            <h1>Welkom, {businessName}</h1>

            <p>Bedankt voor het aanmelden als bedrijf bij CarAndAll.</p>
            <p>Ons personeel zal binnen 48 uw aanvraag voor een bedrijfsaccount bekijken en verwerken met de volgende gegevens:</p>
            <ul>
                <li>Abonnement: {subscription}</li>
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
            <li>Wachtwoord: {password}</li>
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
