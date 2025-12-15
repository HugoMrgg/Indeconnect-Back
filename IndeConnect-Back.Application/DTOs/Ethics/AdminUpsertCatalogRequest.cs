namespace IndeConnect_Back.Application.DTOs.Ethics;

public record AdminUpsertCatalogRequest(
    IEnumerable<UpsertCategoryDto> Categories,
    IEnumerable<UpsertQuestionDto> Questions,
    IEnumerable<UpsertOptionDto> Options
);

public record UpsertCategoryDto(
    long? Id,
    string Key,
    string Label,
    int Order,
    bool IsActive
);

public record UpsertQuestionDto(
    long? Id,
    long CategoryId,
    string Key,
    string Label,
    int Order,
    string AnswerType, // "Single" | "Multiple"
    bool IsActive
);

public record UpsertOptionDto(
    long? Id,
    long QuestionId,
    string Key,
    string Label,
    int Order,
    decimal Score,
    bool IsActive
);