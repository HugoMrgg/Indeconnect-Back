namespace IndeConnect_Back.Application.DTOs.Ethics;

public record ReviewQuestionnaireRequest(
    bool Approve,
    string? RejectionReason
);