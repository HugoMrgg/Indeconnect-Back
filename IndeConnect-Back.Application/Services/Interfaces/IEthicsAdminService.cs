using IndeConnect_Back.Application.DTOs.Ethics;

namespace IndeConnect_Back.Application.Services.Interfaces;

public interface IEthicsAdminService
{
    Task<AdminUpsertCatalogRequest> GetCatalogAsync();
    Task UpsertCatalogAsync(AdminUpsertCatalogRequest request);

    Task ReviewQuestionnaireAsync(long questionnaireId, long adminUserId, ReviewQuestionnaireRequest request);
}