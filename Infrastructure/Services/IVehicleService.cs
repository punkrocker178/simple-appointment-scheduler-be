using Infrastructure.Common;
using Infrastructure.Vehicles.Dtos;

namespace Infrastructure.Services;

public interface IVehicleService
{
    Task<ServiceResult<IReadOnlyList<VehicleResponse>>> GetByCustomerAsync(
        Guid customerId,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<VehicleResponse>> CreateAsync(
        Guid customerId,
        CreateVehicleRequest request,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<VehicleResponse>> UpdateAsync(
        Guid customerId,
        Guid id,
        UpdateVehicleRequest request,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<object>> DeleteAsync(
        Guid customerId,
        Guid id,
        CancellationToken cancellationToken = default);
}
