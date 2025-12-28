namespace IndeConnect_Back.Application.DTOs.Sizes;

public record SizeLookupDto(
    long Id,
    string Name,
    long CategoryId,
    int SortOrder
);