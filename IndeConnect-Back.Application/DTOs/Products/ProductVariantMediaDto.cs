using IndeConnect_Back.Domain.catalog.product;

namespace IndeConnect_Back.Application.DTOs.Products;

public record ProductVariantMediaDto(
    long Id,
    string Url,
    MediaType Type,
    int DisplayOrder,
    bool IsPrimary
);