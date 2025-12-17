namespace IndeConnect_Back.Application.DTOs.Ethics;

public record UpsertQuestionnaireRequest(
    bool Submit,
    IEnumerable<QuestionAnswerDto> Answers
);

public record QuestionAnswerDto(
    long QuestionId,
    IEnumerable<long> OptionIds
);