using IndeConnect_Back.Domain.catalog.product;

namespace IndeConnect_Back.Application.DTOs.Products;

public record ProductMediaDto(
    long Id,
    string Url,
    MediaType Type,
    int DisplayOrder,
    bool IsPrimary
);