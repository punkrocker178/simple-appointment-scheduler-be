using Infrastructure.Appointments;
using Infrastructure.Common;
using Infrastructure.Data;
using Infrastructure.Dealerships.Dtos;
using Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services;

public class DealershipService : IDealershipService
{
    private readonly ApplicationDbContext _db;

    public DealershipService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<ServiceResult<IReadOnlyList<DealershipResponse>>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        var dealerships = await _db.Dealerships
            .AsNoTracking()
            .OrderBy(d => d.Name)
            .Select(d => ToResponse(d))
            .ToListAsync(cancellationToken);

        return ServiceResult<IReadOnlyList<DealershipResponse>>.Ok(dealerships);
    }

    public async Task<ServiceResult<DealershipResponse>> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var dealership = await _db.Dealerships
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.Id == id, cancellationToken);

        if (dealership is null)
        {
            return ServiceResult<DealershipResponse>.NotFound("Dealership not found.");
        }

        return ServiceResult<DealershipResponse>.Ok(ToResponse(dealership));
    }

    public async Task<ServiceResult<DealershipResponse>> CreateAsync(
        CreateDealershipRequest request,
        CancellationToken cancellationToken = default)
    {
        var validationError = ValidateRequest(
            request.Name,
            request.Address,
            request.Phone,
            request.Timezone,
            request.OpenSecondsFromMidnight,
            request.CloseSecondsFromMidnight);
        if (validationError is not null)
        {
            return ServiceResult<DealershipResponse>.BadRequest(validationError);
        }

        var dealership = new Dealership
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim(),
            Address = request.Address.Trim(),
            Phone = request.Phone.Trim(),
            Timezone = request.Timezone.Trim(),
            OpenSecondsFromMidnight = request.OpenSecondsFromMidnight,
            CloseSecondsFromMidnight = request.CloseSecondsFromMidnight
        };

        _db.Dealerships.Add(dealership);
        await _db.SaveChangesAsync(cancellationToken);

        return ServiceResult<DealershipResponse>.Created(ToResponse(dealership));
    }

    public async Task<ServiceResult<DealershipResponse>> UpdateAsync(
        Guid id,
        UpdateDealershipRequest request,
        CancellationToken cancellationToken = default)
    {
        var validationError = ValidateRequest(
            request.Name,
            request.Address,
            request.Phone,
            request.Timezone,
            request.OpenSecondsFromMidnight,
            request.CloseSecondsFromMidnight);
        if (validationError is not null)
        {
            return ServiceResult<DealershipResponse>.BadRequest(validationError);
        }

        var dealership = await _db.Dealerships.FindAsync([id], cancellationToken);
        if (dealership is null)
        {
            return ServiceResult<DealershipResponse>.NotFound("Dealership not found.");
        }

        dealership.Name = request.Name.Trim();
        dealership.Address = request.Address.Trim();
        dealership.Phone = request.Phone.Trim();
        dealership.Timezone = request.Timezone.Trim();
        dealership.OpenSecondsFromMidnight = request.OpenSecondsFromMidnight;
        dealership.CloseSecondsFromMidnight = request.CloseSecondsFromMidnight;

        await _db.SaveChangesAsync(cancellationToken);

        return ServiceResult<DealershipResponse>.Ok(ToResponse(dealership));
    }

    private static string? ValidateRequest(
        string name,
        string address,
        string phone,
        string timezone,
        int openSeconds,
        int closeSeconds)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return "Name is required.";
        }

        if (string.IsNullOrWhiteSpace(address))
        {
            return "Address is required.";
        }

        if (string.IsNullOrWhiteSpace(phone))
        {
            return "Phone is required.";
        }

        if (string.IsNullOrWhiteSpace(timezone))
        {
            return "Timezone is required.";
        }

        return AvailabilityEngine.ValidateBusinessHours(openSeconds, closeSeconds);
    }

    private static DealershipResponse ToResponse(Dealership dealership) =>
        new()
        {
            Id = dealership.Id,
            Name = dealership.Name,
            Address = dealership.Address,
            Phone = dealership.Phone,
            Timezone = dealership.Timezone,
            OpenSecondsFromMidnight = dealership.OpenSecondsFromMidnight,
            CloseSecondsFromMidnight = dealership.CloseSecondsFromMidnight
        };
}
