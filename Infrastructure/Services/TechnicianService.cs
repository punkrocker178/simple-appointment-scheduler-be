using Infrastructure.Common;
using Infrastructure.Data;
using Infrastructure.Entities;
using Infrastructure.Technicians.Dtos;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services;

public class TechnicianService : ITechnicianService
{
    private readonly ApplicationDbContext _db;

    public TechnicianService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<ServiceResult<IReadOnlyList<TechnicianResponse>>> GetByDealershipAsync(
        Guid dealershipId,
        CancellationToken cancellationToken = default)
    {
        if (!await _db.Dealerships.AnyAsync(d => d.Id == dealershipId, cancellationToken))
        {
            return ServiceResult<IReadOnlyList<TechnicianResponse>>.NotFound("Dealership not found.");
        }

        var technicians = await _db.Technicians
            .AsNoTracking()
            .Where(t => t.DealershipId == dealershipId)
            .OrderBy(t => t.LastName)
            .ThenBy(t => t.FirstName)
            .Select(t => ToResponse(t))
            .ToListAsync(cancellationToken);

        return ServiceResult<IReadOnlyList<TechnicianResponse>>.Ok(technicians);
    }

    public async Task<ServiceResult<TechnicianResponse>> CreateAsync(
        Guid dealershipId,
        CreateTechnicianRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!await _db.Dealerships.AnyAsync(d => d.Id == dealershipId, cancellationToken))
        {
            return ServiceResult<TechnicianResponse>.NotFound("Dealership not found.");
        }

        var validationError = ValidateName(request.FirstName, request.LastName);
        if (validationError is not null)
        {
            return ServiceResult<TechnicianResponse>.BadRequest(validationError);
        }

        var technician = new Technician
        {
            Id = Guid.NewGuid(),
            DealershipId = dealershipId,
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            IsActive = true
        };

        _db.Technicians.Add(technician);
        await _db.SaveChangesAsync(cancellationToken);

        return ServiceResult<TechnicianResponse>.Created(ToResponse(technician));
    }

    public async Task<ServiceResult<TechnicianResponse>> UpdateAsync(
        Guid dealershipId,
        Guid id,
        UpdateTechnicianRequest request,
        CancellationToken cancellationToken = default)
    {
        var validationError = ValidateName(request.FirstName, request.LastName);
        if (validationError is not null)
        {
            return ServiceResult<TechnicianResponse>.BadRequest(validationError);
        }

        var technician = await _db.Technicians
            .FirstOrDefaultAsync(t => t.Id == id && t.DealershipId == dealershipId, cancellationToken);

        if (technician is null)
        {
            return ServiceResult<TechnicianResponse>.NotFound("Technician not found.");
        }

        technician.FirstName = request.FirstName.Trim();
        technician.LastName = request.LastName.Trim();
        await _db.SaveChangesAsync(cancellationToken);

        return ServiceResult<TechnicianResponse>.Ok(ToResponse(technician));
    }

    public async Task<ServiceResult<object>> SoftDeleteAsync(
        Guid dealershipId,
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var technician = await _db.Technicians
            .FirstOrDefaultAsync(t => t.Id == id && t.DealershipId == dealershipId, cancellationToken);

        if (technician is null)
        {
            return ServiceResult<object>.NotFound("Technician not found.");
        }

        if (!technician.IsActive)
        {
            return ServiceResult<object>.BadRequest("Technician is already inactive.");
        }

        technician.IsActive = false;
        await _db.SaveChangesAsync(cancellationToken);

        return ServiceResult<object>.NoContent();
    }

    private static string? ValidateName(string firstName, string lastName)
    {
        if (string.IsNullOrWhiteSpace(firstName))
        {
            return "First name is required.";
        }

        if (string.IsNullOrWhiteSpace(lastName))
        {
            return "Last name is required.";
        }

        return null;
    }

    private static TechnicianResponse ToResponse(Technician technician) =>
        new()
        {
            Id = technician.Id,
            DealershipId = technician.DealershipId,
            FirstName = technician.FirstName,
            LastName = technician.LastName,
            IsActive = technician.IsActive
        };
}
