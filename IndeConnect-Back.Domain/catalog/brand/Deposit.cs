namespace IndeConnect_Back.Domain.catalog.brand;

public class Deposit
{
    public string Id { get; private set; } = default!;
    public int Number { get; private set; }
    public string Street { get; private set; } = default!;
    public string PostalCode { get; private set; } = default!;
    
    public string City { get; private set; } = default!;
    public string Country { get; private set; } = default!;
    // Coordonnées GPS
    public double Latitude { get; private set; }
    public double Longitude { get; private set; }

    public long BrandId { get; private set; }
    public Brand Brand { get; private set; } = default!;

    private Deposit() { }

    public Deposit(
        string id,
        int number,
        string street,
        string postalCode,
        string city,
        string country,
        double latitude,
        double longitude,
        long brandId)
    {
        Id = id;
        Number = number;
        Street = street.Trim();
        PostalCode = postalCode.Trim();
        City = city.Trim();
        Country = country.Trim();
        Latitude = latitude;
        Longitude = longitude;
        BrandId = brandId;
    }

    public string GetFullAddress() => $"{Number} {Street}, {PostalCode} {City}, {Country}";
}