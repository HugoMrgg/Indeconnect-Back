using IndeConnect_Back.Application.DTOs.Ethics;

namespace IndeConnect_Back.Application.Services.Interfaces;

public interface IEthicsQuestionnaireService
{
    Task<EthicsFormDto> GetMyEthicsFormAsync(long superVendorUserId);
    Task<EthicsFormDto> UpsertMyQuestionnaireAsync(long superVendorUserId, UpsertQuestionnaireRequest request);
}