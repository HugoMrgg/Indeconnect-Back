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

    public SendGridEmailService(ILogger<SendGridEmailService> logger)
    {
        var apiKey = Environment.GetEnvironmentVariable("SENDGRID_API_KEY");
        if (string.IsNullOrWhiteSpace(apiKey))
            throw new InvalidOperationException("SENDGRID_API_KEY is not configured");

        _client = new SendGridClient(apiKey);
        _logger = logger;
        _fromEmail = Environment.GetEnvironmentVariable("SENDGRID_FROM_EMAIL");
        _fromName = Environment.GetEnvironmentVariable("SENDGRID_FROM_NAME");
    }
    public async Task SendEmailAsync(string toEmail, string subject, string htmlContent)
    {
        try
        {
            var from = new EmailAddress(_fromEmail, _fromName);
            var to = new EmailAddress(toEmail);
            var msg = new SendGridMessage
            {
                From = from,
                Subject = subject,
                HtmlContent = htmlContent
            };
            msg.AddTo(to);

            var response = await _client.SendEmailAsync(msg);

            if (response.StatusCode != System.Net.HttpStatusCode.Accepted)
            {
                _logger.LogError("SendGrid failed: {StatusCode}", response.StatusCode);
                throw new InvalidOperationException($"Failed to send email: {response.StatusCode}");
            }

            _logger.LogInformation("Email sent successfully to {Email}", toEmail);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending email to {Email}", toEmail);
            throw;
        }
    }
}
