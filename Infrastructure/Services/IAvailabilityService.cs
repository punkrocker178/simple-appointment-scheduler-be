using Infrastructure.Appointments.Dtos;
using Infrastructure.Common;

namespace Infrastructure.Services;

public interface IAvailabilityService
{
    Task<ServiceResult<AvailabilityResponse>> GetAvailabilityAsync(
        Guid dealershipId,
        Guid serviceTypeId,
        DateOnly bookingDate,
        CancellationToken cancellationToken = default);
}
