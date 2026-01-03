using IndeConnect_Back.Application.DTOs.Brands;

namespace IndeConnect_Back.Application.Services.Interfaces;

public interface IBrandRequestEmailTemplateService
{
    string GenerateBecomeBrandRequestEmail(BecomeBrandRequestDto request);
}