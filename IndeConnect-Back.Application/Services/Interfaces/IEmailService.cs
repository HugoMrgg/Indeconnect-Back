namespace IndeConnect_Back.Application.Services.Interfaces;

public interface IEmailService
{
    /// <summary>
    /// Envoie un email d'activation avec un lien pour définir le mot de passe
    /// </summary>
    Task SendActivationEmailAsync(string email, string firstName, string activationLink);

    /// <summary>
    /// Envoie un email 
    /// </summary>
    Task SendEmailAsync(string toEmail, string subject, string htmlContent);
}