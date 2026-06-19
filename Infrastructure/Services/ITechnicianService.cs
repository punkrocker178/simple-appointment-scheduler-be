using Infrastructure.Common;
using Infrastructure.Technicians.Dtos;

namespace Infrastructure.Services;

public interface ITechnicianService
{
    Task<ServiceResult<IReadOnlyList<TechnicianResponse>>> GetByDealershipAsync(
        Guid dealershipId,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<TechnicianResponse>> CreateAsync(
        Guid dealershipId,
        CreateTechnicianRequest request,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<TechnicianResponse>> UpdateAsync(
        Guid dealershipId,
        Guid id,
        UpdateTechnicianRequest request,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<object>> SoftDeleteAsync(
        Guid dealershipId,
        Guid id,
        CancellationToken cancellationToken = default);
}
