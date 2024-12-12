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

    public async Task SendWelcomeEmail(string toEmail)
    {

        try
        {
            if (string.IsNullOrEmpty(toEmail) || string.IsNullOrEmpty(_fromEmail))
            {
                throw new ArgumentException("Email addresses cannot be null or empty");
            }
            
            
            using var smtpClient = new SmtpClient(_smtpHost)
            {
                Port = _smtpPort,
                Credentials = new NetworkCredential(_fromEmail, _fromPassword),
                EnableSsl = true
            };

            using var mailMessage = new MailMessage
            {
                From = new MailAddress(_fromEmail),
                Subject = "Welcome to CarAndALl",
                Body = $"Welcome to CarAndAll",
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
}