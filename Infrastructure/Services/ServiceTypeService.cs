using Infrastructure.Common;
using Infrastructure.Data;
using Infrastructure.Entities;
using Infrastructure.ServiceTypes.Dtos;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services;

public class ServiceTypeService : IServiceTypeService
{
    private readonly ApplicationDbContext _db;

    public ServiceTypeService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<ServiceResult<IReadOnlyList<ServiceTypeResponse>>> GetByDealershipAsync(
        Guid dealershipId,
        CancellationToken cancellationToken = default)
    {
        if (!await _db.Dealerships.AnyAsync(d => d.Id == dealershipId, cancellationToken))
        {
            return ServiceResult<IReadOnlyList<ServiceTypeResponse>>.NotFound("Dealership not found.");
        }

        var serviceTypes = await _db.ServiceTypes
            .AsNoTracking()
            .Where(st => st.DealershipId == dealershipId)
            .OrderBy(st => st.Name)
            .Select(st => ToResponse(st))
            .ToListAsync(cancellationToken);

        return ServiceResult<IReadOnlyList<ServiceTypeResponse>>.Ok(serviceTypes);
    }

    public async Task<ServiceResult<ServiceTypeResponse>> CreateAsync(
        Guid dealershipId,
        CreateServiceTypeRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!await _db.Dealerships.AnyAsync(d => d.Id == dealershipId, cancellationToken))
        {
            return ServiceResult<ServiceTypeResponse>.NotFound("Dealership not found.");
        }

        var validationError = ValidateRequest(
            request.SkillId,
            request.Name,
            request.DurationMinutes,
            request.Price);

        if (validationError is not null)
        {
            return ServiceResult<ServiceTypeResponse>.BadRequest(validationError);
        }

        if (!await _db.Skills.AnyAsync(s => s.Id == request.SkillId, cancellationToken))
        {
            return ServiceResult<ServiceTypeResponse>.BadRequest("Skill not found.");
        }

        var serviceType = new ServiceType
        {
            Id = Guid.NewGuid(),
            DealershipId = dealershipId,
            SkillId = request.SkillId,
            Name = request.Name.Trim(),
            Description = string.IsNullOrWhiteSpace(request.Description)
                ? null
                : request.Description.Trim(),
            DurationMinutes = request.DurationMinutes,
            Price = request.Price,
            IsActive = true
        };

        _db.ServiceTypes.Add(serviceType);
        await _db.SaveChangesAsync(cancellationToken);

        return ServiceResult<ServiceTypeResponse>.Created(ToResponse(serviceType));
    }

    public async Task<ServiceResult<ServiceTypeResponse>> UpdateAsync(
        Guid dealershipId,
        Guid id,
        UpdateServiceTypeRequest request,
        CancellationToken cancellationToken = default)
    {
        var validationError = ValidateRequest(
            request.SkillId,
            request.Name,
            request.DurationMinutes,
            request.Price);

        if (validationError is not null)
        {
            return ServiceResult<ServiceTypeResponse>.BadRequest(validationError);
        }

        if (!await _db.Skills.AnyAsync(s => s.Id == request.SkillId, cancellationToken))
        {
            return ServiceResult<ServiceTypeResponse>.BadRequest("Skill not found.");
        }

        var serviceType = await _db.ServiceTypes
            .FirstOrDefaultAsync(st => st.Id == id && st.DealershipId == dealershipId, cancellationToken);

        if (serviceType is null)
        {
            return ServiceResult<ServiceTypeResponse>.NotFound("Service type not found.");
        }

        serviceType.SkillId = request.SkillId;
        serviceType.Name = request.Name.Trim();
        serviceType.Description = string.IsNullOrWhiteSpace(request.Description)
            ? null
            : request.Description.Trim();
        serviceType.DurationMinutes = request.DurationMinutes;
        serviceType.Price = request.Price;

        await _db.SaveChangesAsync(cancellationToken);

        return ServiceResult<ServiceTypeResponse>.Ok(ToResponse(serviceType));
    }

    public async Task<ServiceResult<object>> SoftDeleteAsync(
        Guid dealershipId,
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var serviceType = await _db.ServiceTypes
            .FirstOrDefaultAsync(st => st.Id == id && st.DealershipId == dealershipId, cancellationToken);

        if (serviceType is null)
        {
            return ServiceResult<object>.NotFound("Service type not found.");
        }

        if (!serviceType.IsActive)
        {
            return ServiceResult<object>.BadRequest("Service type is already inactive.");
        }

        serviceType.IsActive = false;
        await _db.SaveChangesAsync(cancellationToken);

        return ServiceResult<object>.NoContent();
    }

    private static string? ValidateRequest(
        Guid skillId,
        string name,
        int durationMinutes,
        decimal price)
    {
        if (skillId == Guid.Empty)
        {
            return "Skill is required.";
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            return "Name is required.";
        }

        if (durationMinutes <= 0)
        {
            return "Duration must be greater than zero.";
        }

        if (price < 0)
        {
            return "Price cannot be negative.";
        }

        return null;
    }

    private static ServiceTypeResponse ToResponse(ServiceType serviceType) =>
        new()
        {
            Id = serviceType.Id,
            DealershipId = serviceType.DealershipId,
            SkillId = serviceType.SkillId,
            Name = serviceType.Name,
            Description = serviceType.Description,
            DurationMinutes = serviceType.DurationMinutes,
            Price = serviceType.Price,
            IsActive = serviceType.IsActive
        };
}
