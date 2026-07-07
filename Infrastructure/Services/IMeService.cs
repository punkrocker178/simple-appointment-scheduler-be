using Infrastructure.Appointments.Dtos;
using Infrastructure.Common;
using Infrastructure.Customers.Dtos;
using Infrastructure.Me.Dtos;
using Infrastructure.Vehicles.Dtos;

namespace Infrastructure.Services;

public interface IMeService
{
    Task<ServiceResult<CustomerResponse>> GetCustomerAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<CustomerResponse>> UpdateCustomerAsync(
        Guid userId,
        UpdateMeCustomerRequest request,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<IReadOnlyList<VehicleResponse>>> GetVehiclesAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<VehicleResponse>> CreateVehicleAsync(
        Guid userId,
        CreateVehicleRequest request,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<VehicleResponse>> UpdateVehicleAsync(
        Guid userId,
        Guid vehicleId,
        UpdateVehicleRequest request,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<IReadOnlyList<AppointmentResponse>>> GetAppointmentsAsync(
        Guid userId,
        CancellationToken cancellationToken = default);
}
