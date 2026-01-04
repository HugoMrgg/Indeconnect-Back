using IndeConnect_Back.Application.DTOs.Brands;
using IndeConnect_Back.Application.Services.Interfaces;
using Microsoft.Extensions.Configuration;

namespace IndeConnect_Back.Infrastructure.Services.Implementations;

public class BrandRequestMailService : IBrandRequestMailService
{
    private readonly IBrandRequestEmailTemplateService _templateService;
    private readonly IEmailService _emailService;
    private readonly string _adminEmail;

    public BrandRequestMailService(
        IBrandRequestEmailTemplateService templateService,
        IEmailService emailService,
        IConfiguration configuration)
    {
        _templateService = templateService;
        _emailService = emailService;
        _adminEmail = configuration["SENDGRID_FROM_EMAIL"];
    }

    public async Task SendBecomeBrandRequestAsync(BecomeBrandRequestDto request)
    {
        var html = _templateService.GenerateBecomeBrandRequestEmail(request);
        var subject = $"[IndeConnect] Demande marque : {request.BrandName}";

        await _emailService.SendEmailAsync(_adminEmail, subject, html);
    }
}