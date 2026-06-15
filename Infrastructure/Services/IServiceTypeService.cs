using Infrastructure.Common;
using Infrastructure.ServiceTypes.Dtos;

namespace Infrastructure.Services;

public interface IServiceTypeService
{
    Task<ServiceResult<IReadOnlyList<ServiceTypeResponse>>> GetByDealershipAsync(
        Guid dealershipId,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<ServiceTypeResponse>> CreateAsync(
        Guid dealershipId,
        CreateServiceTypeRequest request,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<ServiceTypeResponse>> UpdateAsync(
        Guid dealershipId,
        Guid id,
        UpdateServiceTypeRequest request,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<object>> SoftDeleteAsync(
        Guid dealershipId,
        Guid id,
        CancellationToken cancellationToken = default);
}
