namespace IndeConnect_Back.Application.DTOs.Ethics;

public record AdminCatalogDto(
    IReadOnlyList<AdminCategoryDto> Categories,
    IReadOnlyList<AdminQuestionDto> Questions,
    IReadOnlyList<AdminOptionDto> Options
);

public record AdminCategoryDto(
    long Id,
    string Key,
    string Label,
    int Order,
    bool IsActive
);

public record AdminQuestionDto(
    long Id,
    long CategoryId,
    string CategoryKey,
    string Key,
    string Label,
    int Order,
    string AnswerType,
    bool IsActive
);

public record AdminOptionDto(
    long Id,
    long QuestionId,
    string QuestionKey,
    string Key,
    string Label,
    int Order,
    decimal Score,
    bool IsActive
);