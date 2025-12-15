namespace IndeConnect_Back.Domain.user;

/**
 * Represents a User's shipping address
 */
public class ShippingAddress
{
    public long Id { get; private set; }
    public long UserId { get; private set; }
    public User User { get; private set; } = default!;

    public string Street { get; private set; } = default!;
    public string Number { get; private set; } = default!;
    public string PostalCode { get; private set; } = default!;
    public string City { get; private set; } = default!;
    public string Country { get; private set; } = "BE";
    public string? Extra { get; private set; }

    public bool IsDefault { get; private set; } = false;

    private ShippingAddress() { }

    public ShippingAddress(
        long userId,
        string street,
        string number,
        string postalCode,
        string city,
        string country = "BE",
        bool isDefault = false,
        string? extra = null)
    {
        UserId = userId;
        Street = street ?? throw new ArgumentNullException(nameof(street));
        Number = number ?? throw new ArgumentNullException(nameof(number));
        PostalCode = postalCode ?? throw new ArgumentNullException(nameof(postalCode));
        City = city ?? throw new ArgumentNullException(nameof(city));
        Country = country ?? "BE";
        IsDefault = isDefault;
        Extra = extra;
    }

    public void Update(
        string? street = null,
        string? number = null,
        string? postalCode = null,
        string? city = null,
        string? country = null,
        string? extra = null)
    {
        if (street != null) Street = street;
        if (number != null) Number = number;
        if (postalCode != null) PostalCode = postalCode;
        if (city != null) City = city;
        if (country != null) Country = country;
        if (extra != null) Extra = extra;
    }

    public void SetAsDefault()
    {
        IsDefault = true;
    }

    public void UnsetAsDefault()
    {
        IsDefault = false;
    }
}
