using IndeConnect_Back.Application.DTOs.Brands;

namespace IndeConnect_Back.Application.Services.Interfaces;

public interface IBrandRequestMailService
{
    Task SendBecomeBrandRequestAsync(BecomeBrandRequestDto request);
}