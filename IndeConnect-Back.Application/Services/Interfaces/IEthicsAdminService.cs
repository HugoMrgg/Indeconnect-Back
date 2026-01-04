using IndeConnect_Back.Application.DTOs.Ethics;

namespace IndeConnect_Back.Application.Services.Interfaces;

public interface IEthicsAdminService
{
    Task<AdminCatalogDto> GetCatalogAsync();
    Task<AdminCatalogDto> UpsertCatalogAsync(AdminUpsertCatalogRequest request);
    Task PublishDraftAsync();

    Task ReviewQuestionnaireAsync(long questionnaireId, long adminUserId, ReviewQuestionnaireRequest request);
}