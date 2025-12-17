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
    string CategoryKey,   // ✅ lien par Key
    string Key,
    string Label,
    int Order,
    string AnswerType,
    bool IsActive
);

public record UpsertOptionDto(
    long? Id,
    string QuestionKey,   // ✅ lien par Key
    string Key,
    string Label,
    int Order,
    decimal Score,
    bool IsActive
);