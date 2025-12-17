namespace IndeConnect_Back.Application.DTOs.Brands;

public record DepositDto(
    string Id,
    string FullAddress,
    double? DistanceKm,
    string City
);