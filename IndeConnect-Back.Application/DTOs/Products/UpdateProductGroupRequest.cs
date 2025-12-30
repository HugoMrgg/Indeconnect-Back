namespace IndeConnect_Back.Application.DTOs.Products;

public record UpdateProductGroupRequest(
    string Name,
    string BaseDescription,
    long CategoryId
);
