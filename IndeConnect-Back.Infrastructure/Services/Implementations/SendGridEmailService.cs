using IndeConnect_Back.Application.Services.Interfaces;
using SendGrid;
using SendGrid.Helpers.Mail;
using Microsoft.Extensions.Logging;

namespace IndeConnect_Back.Infrastructure.Services.Implementations;

public class SendGridEmailService : IEmailService
{
    private readonly SendGridClient _client;
    private readonly ILogger<SendGridEmailService> _logger;
    private readonly string _fromEmail;
    private readonly string _fromName;
    private readonly string _frontendUrl;

    public SendGridEmailService(ILogger<SendGridEmailService> logger)
    {
        var apiKey = Environment.GetEnvironmentVariable("SENDGRID_API_KEY");
        if (string.IsNullOrWhiteSpace(apiKey))
            throw new InvalidOperationException("SENDGRID_API_KEY is not configured");

        _client = new SendGridClient(apiKey);
        _logger = logger;
        _fromEmail = Environment.GetEnvironmentVariable("SENDGRID_FROM_EMAIL");
        _fromName = Environment.GetEnvironmentVariable("SENDGRID_FROM_NAME");
        _frontendUrl = Environment.GetEnvironmentVariable("FRONTEND_URL");
    }

    public async Task SendActivationEmailAsync(string email, string firstName, string token)
    {
        var subject = "Activez votre compte IndeConnect";
        
        // Génère le lien pour le frontend
        var activationLink = $"{_frontendUrl}/set-password?token={token}";
        
        var htmlContent = $@"
            <!DOCTYPE html>
            <html>
            <head>
                <style>
                    body {{ font-family: Arial, sans-serif; }}
                    .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                    .header {{ background-color: #000; color: #fff; padding: 20px; text-align: center; }}
                    .content {{ padding: 20px; background-color: #f9f9f9; }}
                    .button {{ display: inline-block; background-color: #000; color: #fff; padding: 12px 24px; text-decoration: none; border-radius: 5px; margin-top: 20px; }}
                    .footer {{ text-align: center; padding-top: 20px; font-size: 12px; color: #666; }}
                </style>
            </head>
            <body>
                <div class=""container"">
                    <div class=""header"">
                        <h1>IndeConnect</h1>
                    </div>
                    <div class=""content"">
                        <p>Bonjour {firstName},</p>
                        <p>Un compte a été créé pour vous sur IndeConnect.</p>
                        <p>Cliquez sur le lien ci-dessous pour activer votre compte et définir votre mot de passe :</p>
                        <a href=""{activationLink}"" class=""button"">Activer mon compte</a>
                        <p style=""margin-top: 20px; color: #666; font-size: 12px;"">
                            Ce lien expire dans 24 heures.
                        </p>
                    </div>
                </div>
            </body>
            </html>
        ";
        
        await SendEmailAsync(email, subject, htmlContent);
    }

    public async Task SendEmailAsync(string toEmail, string subject, string htmlContent)
    {
        try
        {
            var from = new EmailAddress(_fromEmail, _fromName);
            var to = new EmailAddress(toEmail);
            var msg = new SendGridMessage()
            {
                From = from,
                Subject = subject
            };
            msg.AddTo(to);

            var response = await _client.SendEmailAsync(msg);

            if (response.StatusCode != System.Net.HttpStatusCode.Accepted)
            {
                _logger.LogError($"SendGrid failed: {response.StatusCode}");
                throw new InvalidOperationException($"Failed to send email: {response.StatusCode}");
            }

            _logger.LogInformation($"Email sent successfully to {toEmail}");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error sending email to {toEmail}: {ex.Message}");
            throw;
        }
    }
}
