using IndeConnect_Back.Application.Services.Interfaces;
using IndeConnect_Back.Domain.catalog.brand;

namespace IndeConnect_Back.Infrastructure.Services.Implementations;

public class DepositService : IDepositService
{
    private readonly AppDbContext _context;
    private readonly IGeocodeService _geocodeService;

    public DepositService(AppDbContext context, IGeocodeService geocodeService)
    {
        _context = context;
        _geocodeService = geocodeService;
    }

    public async Task<Deposit> CreateDepositAsync(
        string id,
        int number,
        string street,
        string postalCode,
        long brandId)
    {
        var fullAddress = $"{number} {street}, {postalCode}";

        var coords = await _geocodeService.GeocodeAddressAsync(fullAddress);

        if (coords == null)
            throw new InvalidOperationException($"Unable to geocode address: {fullAddress}");

        var deposit = new Deposit(
            id,
            number,
            street,
            postalCode,
            coords.Value.Latitude,
            coords.Value.Longitude,
            brandId
        );

        _context.Add(deposit);
        await _context.SaveChangesAsync();

        return deposit;
    }
}