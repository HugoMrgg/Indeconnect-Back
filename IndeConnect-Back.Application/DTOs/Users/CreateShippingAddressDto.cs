namespace IndeConnect_Back.Application.DTOs.Users;

public class CreateShippingAddressDto
{
    public string Street { get; set; } = string.Empty;
    public string Number { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Country { get; set; } = "BE";
    public string? Extra { get; set; }
    public bool IsDefault { get; set; }
}