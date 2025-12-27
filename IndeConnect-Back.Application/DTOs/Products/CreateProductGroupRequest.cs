namespace IndeConnect_Back.Application.DTOs.Products;

public record CreateProductGroupRequest(
    string Name,
    string BaseDescription,
    long CategoryId
);
