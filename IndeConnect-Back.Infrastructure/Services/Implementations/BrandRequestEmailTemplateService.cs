using IndeConnect_Back.Application.DTOs.Brands;
using IndeConnect_Back.Application.Services.Interfaces;
using Microsoft.Extensions.Configuration;

namespace IndeConnect_Back.Infrastructure.Services.Implementations;

public class BrandRequestEmailTemplateService : IBrandRequestEmailTemplateService
{
    private readonly string _frontendUrl;

    public BrandRequestEmailTemplateService(IConfiguration configuration)
    {
        _frontendUrl = configuration["FRONTEND_URL"] ?? "https://indeconnect.com";
    }

    public string GenerateBecomeBrandRequestEmail(BecomeBrandRequestDto request)
    {
        string Safe(string? s) => string.IsNullOrWhiteSpace(s) ? "—" : s.Trim();

        var content = $@"
            <p>Une nouvelle demande <strong>« Devenir une marque »</strong> a été soumise via IndeConnect.</p>

            <div style=""background-color: #fff; padding: 15px; border-radius: 5px; margin: 20px 0;"">
                <h3 style=""margin-top: 0; color: #000;"">Informations</h3>
                <p style=""margin: 6px 0;""><strong>Marque :</strong> {Safe(request.BrandName)}</p>
                <p style=""margin: 6px 0;""><strong>Contact :</strong> {Safe(request.ContactName)}</p>
                <p style=""margin: 6px 0;""><strong>Email :</strong> <a href=""mailto:{request.Email}"">{request.Email}</a></p>
                <p style=""margin: 6px 0;""><strong>Site web :</strong> {Safe(request.Website)}</p>
            </div>

            <h3 style=""color: #000;"">Message</h3>
            <div style=""background-color: #fff; padding: 15px; border-radius: 5px;"">
                <p style=""margin: 0; white-space: pre-wrap;"">{Safe(request.Message)}</p>
            </div>

            <div style=""margin-top: 20px;"">
                <a href=""mailto:{request.Email}?subject=IndeConnect%20-%20Demande%20de%20marque"" class=""button"">
                    Répondre au demandeur
                </a>
                <a href=""{_frontendUrl}/admin/accounts"" class=""button"" style=""margin-left: 10px;"">
                    Ouvrir l’admin
                </a>
            </div>
        ";

        return BuildEmailTemplate(
            title: "Nouvelle demande : Devenir une marque",
            content: content,
            subtitle: $"Demande marque • {request.BrandName}"
        );
    }

    private static string BuildEmailTemplate(string title, string content, string subtitle)
    {
        return $@"
        <!DOCTYPE html>
        <html>
        <head>
            <style>
                body {{ font-family: Arial, sans-serif; margin: 0; padding: 0; }}
                .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                .header {{ background-color: #000; color: #fff; padding: 20px; text-align: center; }}
                .content {{ padding: 20px; background-color: #f9f9f9; }}
                .button {{
                    display: inline-block;
                    background-color: #000;
                    color: #fff;
                    padding: 12px 24px;
                    text-decoration: none;
                    border-radius: 5px;
                    margin-top: 20px;
                }}
                .footer {{ text-align: center; padding-top: 20px; font-size: 12px; color: #666; }}
            </style>
        </head>
        <body>
            <div class=""container"">
                <div class=""header"">
                    <h1>IndeConnect</h1>
                    <p style=""margin: 5px 0; font-size: 14px;"">{subtitle}</p>
                </div>
                <div class=""content"">
                    <h2 style=""color: #000; margin-top: 0;"">{title}</h2>
                    {content}
                </div>
                <div class=""footer"">
                    <p>&copy; 2025 IndeConnect. Tous droits réservés.</p>
                </div>
            </div>
        </body>
        </html>";
    }
}
