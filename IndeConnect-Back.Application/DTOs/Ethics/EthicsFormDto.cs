namespace IndeConnect_Back.Application.DTOs.Ethics;

public record EthicsFormDto(
    long? QuestionnaireId,
    string Status,
    IEnumerable<EthicsCategoryDto> Categories
);

public record EthicsCategoryDto(
    long Id,
    string Key,
    string Label,
    int Order,
    IEnumerable<EthicsQuestionDto> Questions
);

public record EthicsQuestionDto(
    long Id,
    string Key,
    string Label,
    int Order,
    string AnswerType, // "Single" | "Multiple"
    IEnumerable<EthicsOptionDto> Options,
    IEnumerable<long> SelectedOptionIds
);

public record EthicsOptionDto(
    long Id,
    string Key,
    string Label,
    int Order,
    decimal Score
);