namespace IndeConnect_Back.Domain.user;
/**
 * Represents a User's shipping adress
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
    public ShippingAddress(long userId, string street, string number, string postalCode, string city, string country, bool isDefault = false, string? extra = null)
    {
        UserId = userId;
        Street = street;
        Number = number;
        PostalCode = postalCode;
        City = city;
        Country = country;
        IsDefault = isDefault;
        Extra = extra;
    }
}
