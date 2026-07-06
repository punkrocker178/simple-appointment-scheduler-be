using Infrastructure.Common;
using Infrastructure.Data;
using Infrastructure.Entities;
using Infrastructure.Vehicles.Dtos;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services;

public class VehicleService : IVehicleService
{
    private readonly ApplicationDbContext _db;

    public VehicleService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<ServiceResult<IReadOnlyList<VehicleResponse>>> GetByCustomerAsync(
        Guid customerId,
        CancellationToken cancellationToken = default)
    {
        if (!await _db.Customers.AnyAsync(c => c.Id == customerId, cancellationToken))
        {
            return ServiceResult<IReadOnlyList<VehicleResponse>>.NotFound("Customer not found.");
        }

        var vehiclesWithAppointments = await GetVehicleIdsWithAppointmentsAsync(cancellationToken);

        var vehicles = await _db.Vehicles
            .AsNoTracking()
            .Where(v => v.CustomerId == customerId)
            .OrderBy(v => v.Make)
            .ThenBy(v => v.Model)
            .ToListAsync(cancellationToken);

        var responses = vehicles
            .Select(v => ToResponse(v, canDelete: !vehiclesWithAppointments.Contains(v.Id)))
            .ToList();

        return ServiceResult<IReadOnlyList<VehicleResponse>>.Ok(responses);
    }

    public async Task<ServiceResult<VehicleResponse>> CreateAsync(
        Guid customerId,
        CreateVehicleRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!await _db.Customers.AnyAsync(c => c.Id == customerId, cancellationToken))
        {
            return ServiceResult<VehicleResponse>.NotFound("Customer not found.");
        }

        var validationError = ValidateRequest(request.Make, request.Model, request.Year);
        if (validationError is not null)
        {
            return ServiceResult<VehicleResponse>.BadRequest(validationError);
        }

        var vehicle = new Vehicle
        {
            Id = Guid.NewGuid(),
            CustomerId = customerId,
            Make = request.Make.Trim(),
            Model = request.Model.Trim(),
            Year = request.Year
        };

        _db.Vehicles.Add(vehicle);
        await _db.SaveChangesAsync(cancellationToken);

        return ServiceResult<VehicleResponse>.Created(ToResponse(vehicle, canDelete: true));
    }

    public async Task<ServiceResult<VehicleResponse>> UpdateAsync(
        Guid customerId,
        Guid id,
        UpdateVehicleRequest request,
        CancellationToken cancellationToken = default)
    {
        var validationError = ValidateRequest(request.Make, request.Model, request.Year);
        if (validationError is not null)
        {
            return ServiceResult<VehicleResponse>.BadRequest(validationError);
        }

        var vehicle = await _db.Vehicles
            .FirstOrDefaultAsync(v => v.Id == id && v.CustomerId == customerId, cancellationToken);

        if (vehicle is null)
        {
            return ServiceResult<VehicleResponse>.NotFound("Vehicle not found.");
        }

        vehicle.Make = request.Make.Trim();
        vehicle.Model = request.Model.Trim();
        vehicle.Year = request.Year;

        await _db.SaveChangesAsync(cancellationToken);

        var canDelete = !await HasAppointmentsAsync(id, cancellationToken);
        return ServiceResult<VehicleResponse>.Ok(ToResponse(vehicle, canDelete));
    }

    public async Task<ServiceResult<object>> DeleteAsync(
        Guid customerId,
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var vehicle = await _db.Vehicles
            .FirstOrDefaultAsync(v => v.Id == id && v.CustomerId == customerId, cancellationToken);

        if (vehicle is null)
        {
            return ServiceResult<object>.NotFound("Vehicle not found.");
        }

        if (await HasAppointmentsAsync(id, cancellationToken))
        {
            return ServiceResult<object>.Conflict("Vehicle has appointments and cannot be deleted.");
        }

        _db.Vehicles.Remove(vehicle);
        await _db.SaveChangesAsync(cancellationToken);

        return ServiceResult<object>.NoContent();
    }

    private async Task<HashSet<Guid>> GetVehicleIdsWithAppointmentsAsync(CancellationToken cancellationToken)
    {
        var vehicleIds = await _db.Appointments
            .Select(a => EF.Property<Guid>(a, "VehicleId"))
            .Distinct()
            .ToListAsync(cancellationToken);

        return vehicleIds.ToHashSet();
    }

    private Task<bool> HasAppointmentsAsync(Guid vehicleId, CancellationToken cancellationToken) =>
        _db.Appointments.AnyAsync(a => EF.Property<Guid>(a, "VehicleId") == vehicleId, cancellationToken);

    private static string? ValidateRequest(string make, string model, int year)
    {
        if (string.IsNullOrWhiteSpace(make))
        {
            return "Make is required.";
        }

        if (string.IsNullOrWhiteSpace(model))
        {
            return "Model is required.";
        }

        var maxYear = DateTime.UtcNow.Year + 1;
        if (year < 1900 || year > maxYear)
        {
            return $"Year must be between 1900 and {maxYear}.";
        }

        return null;
    }

    private static VehicleResponse ToResponse(Vehicle vehicle, bool canDelete) =>
        new()
        {
            Id = vehicle.Id,
            CustomerId = vehicle.CustomerId,
            Make = vehicle.Make,
            Model = vehicle.Model,
            Year = vehicle.Year,
            CanDelete = canDelete
        };
}
