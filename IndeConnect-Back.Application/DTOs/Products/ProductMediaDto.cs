using IndeConnect_Back.Domain.catalog.product;

namespace IndeConnect_Back.Application.DTOs.Products;

public record ProductMediaDto(
    string Url,
    MediaType Type,
    int DisplayOrder,
    bool IsPrimary
);