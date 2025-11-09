using IndeConnect_Back.Domain.catalog.brand;

namespace IndeConnect_Back.Application.Services.Interfaces;

public interface IDepositService
{
    Task<Deposit> CreateDepositAsync(
        string id,
        int number,
        string street,
        string postalCode,
        long brandId);
}