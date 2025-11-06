namespace IndeConnect_Back.Domain.catalog.brand;

public class Deposit
{
    public string Id { get; private set; } = default!; 
    public int Number { get; private set; }
    public string Street { get; private set; } = default!;
    public string PostalCode { get; private set; } = default!;

    public long BrandId { get; private set; }
    public Brand Brand { get; private set; } = default!;

    private Deposit() { }

    public Deposit(string id, int number, string street, string postalCode, long brandId)
    {
        Id = id;
        Number = number;
        Street = street.Trim();
        PostalCode = postalCode.Trim();
        BrandId = brandId;
    }
}