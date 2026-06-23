using Infrastructure.Common;
using Infrastructure.Customers.Dtos;

namespace Infrastructure.Services;

public interface ICustomerService
{
    Task<ServiceResult<IReadOnlyList<CustomerResponse>>> GetAllAsync(
        CancellationToken cancellationToken = default);

    Task<ServiceResult<CustomerResponse>> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<CustomerResponse>> CreateAsync(
        CreateCustomerRequest request,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<CustomerResponse>> UpdateAsync(
        Guid id,
        UpdateCustomerRequest request,
        CancellationToken cancellationToken = default);
}
