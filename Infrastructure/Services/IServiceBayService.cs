using Infrastructure.Common;
using Infrastructure.ServiceBays.Dtos;

namespace Infrastructure.Services;

public interface IServiceBayService
{
    Task<ServiceResult<IReadOnlyList<ServiceBayResponse>>> GetByDealershipAsync(
        Guid dealershipId,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<ServiceBayResponse>> CreateAsync(
        Guid dealershipId,
        CreateServiceBayRequest request,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<ServiceBayResponse>> UpdateAsync(
        Guid dealershipId,
        Guid id,
        UpdateServiceBayRequest request,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<object>> SoftDeleteAsync(
        Guid dealershipId,
        Guid id,
        CancellationToken cancellationToken = default);
}
