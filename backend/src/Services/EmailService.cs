using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;
using WPR.Data;

namespace WPR.Services;

public class EmailService
{
    private readonly EnvConfig _envConfig; // Instantie van EnConfig om environment variabelen op te halen.
    private readonly string _smtpHost;
    private readonly int _smtpPort;
    private readonly string _fromEmail;
    private readonly string _fromPassword;

    public EmailService(EnvConfig envConfig)
    {
        _smtpHost = envConfig.Get("SMTP_HOST");
        _smtpPort = int.Parse(envConfig.Get("SMTP_PORT"));
        _fromEmail = envConfig.Get("SMTP_FROM_EMAIL");
        _fromPassword = envConfig.Get("SMTP_FROM_PASSWORD");
    }

    public async Task SendConfirmationEmail(string toEmail, string confirmationLink)
    {
        var smtpClient = new SmtpClient(_smtpHost)
        {
            Port = _smtpPort,
            Credentials = new NetworkCredential(_fromEmail, _fromPassword),
            EnableSsl = true
        };

        var mailMessage = new MailMessage
        {
            From = new MailAddress(_fromEmail),
            Subject = "Confirm your email",
            Body = $"Please click the link below to confirm your email address: {confirmationLink}",
            IsBodyHtml = true
        };
        
        mailMessage.To.Add(toEmail);
        
        await smtpClient.SendMailAsync(mailMessage);
    }
}