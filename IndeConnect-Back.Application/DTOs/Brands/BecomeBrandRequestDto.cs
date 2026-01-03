namespace IndeConnect_Back.Application.DTOs.Brands;

public record BecomeBrandRequestDto(
    string BrandName,
    string? ContactName,
    string Email,
    string? Website,
    string? Message
);