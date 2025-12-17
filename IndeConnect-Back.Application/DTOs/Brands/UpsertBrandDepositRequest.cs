namespace IndeConnect_Back.Application.DTOs.Brands;

public class UpsertBrandDepositRequest
{
    public int Number { get; set; }
    public string Street { get; set; } = default!;
    public string PostalCode { get; set; } = default!;
    public string City { get; set; } = default!;
    public string Country { get; set; } = default!;
}