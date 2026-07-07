using Infrastructure.Booking.Dtos;
using Infrastructure.Common;

namespace Infrastructure.Services;

public interface IDefaultDealershipService
{
    Task<ServiceResult<DefaultDealershipResponse>> GetDefaultDealershipAsync(
        CancellationToken cancellationToken = default);
}
