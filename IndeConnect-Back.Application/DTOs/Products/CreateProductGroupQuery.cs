namespace IndeConnect_Back.Application.DTOs.Products;

public record CreateProductGroupQuery(
    string Name,
    string BaseDescription,
    long BrandId,
    long CategoryId
);