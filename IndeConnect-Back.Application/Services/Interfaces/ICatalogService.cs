using IndeConnect_Back.Application.DTOs.Ethics;

namespace IndeConnect_Back.Application.Services.Interfaces;

public interface ICatalogService
{
    Task<AdminCatalogDto> GetCatalogAsync();
    Task<AdminCatalogDto> UpsertCatalogAsync(AdminUpsertCatalogRequest request);
}