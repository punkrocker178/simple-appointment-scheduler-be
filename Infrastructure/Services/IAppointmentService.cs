using Infrastructure.Appointments.Dtos;
using Infrastructure.Common;

namespace Infrastructure.Services;

public interface IAppointmentService
{
    Task<ServiceResult<AppointmentResponse>> CreateAsync(
        CreateAppointmentRequest request,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<AppointmentResponse>> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<IReadOnlyList<AppointmentResponse>>> GetByCustomerAsync(
        Guid customerId,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<IReadOnlyList<AppointmentResponse>>> GetByDealershipAndDateAsync(
        Guid dealershipId,
        DateOnly date,
        CancellationToken cancellationToken = default);
}
