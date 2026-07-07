using Infrastructure.Appointments.Dtos;
using Infrastructure.Auth;
using Infrastructure.Common;

namespace Infrastructure.Services;

public interface IAppointmentService
{
    Task<ServiceResult<AppointmentResponse>> CreateAsync(
        CreateAppointmentRequest request,
        AppointmentCallerContext caller,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<AppointmentResponse>> GetByIdAsync(
        Guid id,
        AppointmentCallerContext caller,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<IReadOnlyList<AppointmentResponse>>> GetByCustomerAsync(
        Guid customerId,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<IReadOnlyList<AppointmentResponse>>> GetByDealershipAndDateAsync(
        Guid dealershipId,
        DateOnly date,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<AppointmentResponse>> UpdateStatusAsync(
        Guid id,
        UpdateAppointmentStatusRequest request,
        AppointmentCallerContext caller,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<AppointmentResponse>> CancelAsync(
        Guid id,
        CancelAppointmentRequest request,
        AppointmentCallerContext caller,
        CancellationToken cancellationToken = default);
}
