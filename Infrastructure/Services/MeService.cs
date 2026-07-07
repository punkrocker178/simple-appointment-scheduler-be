using Infrastructure.Appointments.Dtos;
using Infrastructure.Common;
using Infrastructure.Customers.Dtos;
using Infrastructure.Data;
using Infrastructure.Me.Dtos;
using Infrastructure.Vehicles.Dtos;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services;

public class MeService : IMeService
{
    private readonly ApplicationDbContext _db;
    private readonly IVehicleService _vehicleService;
    private readonly IAppointmentService _appointmentService;

    public MeService(
        ApplicationDbContext db,
        IVehicleService vehicleService,
        IAppointmentService appointmentService)
    {
        _db = db;
        _vehicleService = vehicleService;
        _appointmentService = appointmentService;
    }

    public async Task<ServiceResult<CustomerResponse>> GetCustomerAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var customerId = await ResolveCustomerIdAsync(userId, cancellationToken);
        if (customerId is null)
        {
            return ServiceResult<CustomerResponse>.NotFound("Customer profile not found.");
        }

        var customer = await _db.Customers
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == customerId, cancellationToken);

        if (customer is null)
        {
            return ServiceResult<CustomerResponse>.NotFound("Customer profile not found.");
        }

        return ServiceResult<CustomerResponse>.Ok(ToCustomerResponse(customer));
    }

    public async Task<ServiceResult<CustomerResponse>> UpdateCustomerAsync(
        Guid userId,
        UpdateMeCustomerRequest request,
        CancellationToken cancellationToken = default)
    {
        var customerId = await ResolveCustomerIdAsync(userId, cancellationToken);
        if (customerId is null)
        {
            return ServiceResult<CustomerResponse>.NotFound("Customer profile not found.");
        }

        var customer = await _db.Customers.FindAsync([customerId], cancellationToken);
        if (customer is null)
        {
            return ServiceResult<CustomerResponse>.NotFound("Customer profile not found.");
        }

        customer.Phone = request.Phone.Trim();
        await _db.SaveChangesAsync(cancellationToken);

        return ServiceResult<CustomerResponse>.Ok(ToCustomerResponse(customer));
    }

    public async Task<ServiceResult<IReadOnlyList<VehicleResponse>>> GetVehiclesAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var customerId = await ResolveCustomerIdAsync(userId, cancellationToken);
        if (customerId is null)
        {
            return ServiceResult<IReadOnlyList<VehicleResponse>>.NotFound("Customer profile not found.");
        }

        return await _vehicleService.GetByCustomerAsync(customerId.Value, cancellationToken);
    }

    public async Task<ServiceResult<VehicleResponse>> CreateVehicleAsync(
        Guid userId,
        CreateVehicleRequest request,
        CancellationToken cancellationToken = default)
    {
        var customerId = await ResolveCustomerIdAsync(userId, cancellationToken);
        if (customerId is null)
        {
            return ServiceResult<VehicleResponse>.NotFound("Customer profile not found.");
        }

        return await _vehicleService.CreateAsync(customerId.Value, request, cancellationToken);
    }

    public async Task<ServiceResult<VehicleResponse>> UpdateVehicleAsync(
        Guid userId,
        Guid vehicleId,
        UpdateVehicleRequest request,
        CancellationToken cancellationToken = default)
    {
        var customerId = await ResolveCustomerIdAsync(userId, cancellationToken);
        if (customerId is null)
        {
            return ServiceResult<VehicleResponse>.NotFound("Customer profile not found.");
        }

        return await _vehicleService.UpdateAsync(customerId.Value, vehicleId, request, cancellationToken);
    }

    public async Task<ServiceResult<IReadOnlyList<AppointmentResponse>>> GetAppointmentsAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var customerId = await ResolveCustomerIdAsync(userId, cancellationToken);
        if (customerId is null)
        {
            return ServiceResult<IReadOnlyList<AppointmentResponse>>.NotFound("Customer profile not found.");
        }

        return await _appointmentService.GetByCustomerAsync(customerId.Value, cancellationToken);
    }

    private async Task<Guid?> ResolveCustomerIdAsync(Guid userId, CancellationToken cancellationToken) =>
        await _db.Users
            .AsNoTracking()
            .Where(u => u.Id == userId)
            .Select(u => u.CustomerId)
            .FirstOrDefaultAsync(cancellationToken);

    private static CustomerResponse ToCustomerResponse(Infrastructure.Entities.Customer customer) =>
        new()
        {
            Id = customer.Id,
            FirstName = customer.FirstName,
            LastName = customer.LastName,
            Email = customer.Email,
            Phone = customer.Phone,
            CreatedAt = customer.CreatedAt
        };
}
