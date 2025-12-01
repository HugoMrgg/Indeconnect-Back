namespace IndeConnect_Back.Application.Services.Interfaces;

public interface IEmailService
{
    /// <summary>
    /// Envoie un email 
    /// </summary>
    Task SendEmailAsync(string toEmail, string subject, string htmlContent);
}