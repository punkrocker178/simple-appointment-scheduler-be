using Infrastructure.Common;
using Infrastructure.Dealerships.Dtos;

namespace Infrastructure.Services;

public interface IDealershipService
{
    Task<ServiceResult<IReadOnlyList<DealershipResponse>>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<ServiceResult<DealershipResponse>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ServiceResult<DealershipResponse>> CreateAsync(CreateDealershipRequest request, CancellationToken cancellationToken = default);
    Task<ServiceResult<DealershipResponse>> UpdateAsync(Guid id, UpdateDealershipRequest request, CancellationToken cancellationToken = default);
}
