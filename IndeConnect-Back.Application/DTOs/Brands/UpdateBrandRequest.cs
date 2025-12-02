namespace IndeConnect_Back.Application.DTOs.Brands;

public class UpdateBrandRequest
{
    public string? Name { get; set; }
    public string? LogoUrl { get; set; }
    public string? BannerUrl { get; set; }
    public string? Description { get; set; }
    public string? AboutUs { get; set; }
    public string? WhereAreWe { get; set; }
    public string? OtherInfo { get; set; }
    public string? Contact { get; set; }
    public string? PriceRange { get; set; }
}