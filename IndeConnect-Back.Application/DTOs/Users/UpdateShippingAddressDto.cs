namespace IndeConnect_Back.Application.DTOs.Users;


public class UpdateShippingAddressDto
{
    public string? Street { get; set; }
    public string? Number { get; set; }
    public string? PostalCode { get; set; }
    public string? City { get; set; }
    public string? Country { get; set; }
    public string? Extra { get; set; }
    public bool? IsDefault { get; set; }
}