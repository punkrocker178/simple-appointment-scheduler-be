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
            .Include(t => t.TechnicianSkills)
            .ThenInclude(ts => ts.Skill)
            .Where(t => t.DealershipId == dealershipId)
            .OrderBy(t => t.LastName)
            .ThenBy(t => t.FirstName)
            .ToListAsync(cancellationToken);

        return ServiceResult<IReadOnlyList<TechnicianResponse>>.Ok(
            technicians.Select(ToResponseProjection).ToList());
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

        var skillIds = request.SkillIds ?? [];
        var skillValidationError = await ValidateSkillIdsAsync(skillIds, cancellationToken);
        if (skillValidationError is not null)
        {
            return ServiceResult<TechnicianResponse>.BadRequest(skillValidationError);
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
        await SyncSkillsAsync(technician.Id, skillIds, cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);

        var response = await GetResponseAsync(technician.Id, cancellationToken);
        return ServiceResult<TechnicianResponse>.Created(response);
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

        if (request.SkillIds is not null)
        {
            var skillValidationError = await ValidateSkillIdsAsync(request.SkillIds, cancellationToken);
            if (skillValidationError is not null)
            {
                return ServiceResult<TechnicianResponse>.BadRequest(skillValidationError);
            }

            await SyncSkillsAsync(technician.Id, request.SkillIds, cancellationToken);
        }

        technician.FirstName = request.FirstName.Trim();
        technician.LastName = request.LastName.Trim();
        await _db.SaveChangesAsync(cancellationToken);

        var response = await GetResponseAsync(technician.Id, cancellationToken);
        return ServiceResult<TechnicianResponse>.Ok(response);
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

    private async Task<string?> ValidateSkillIdsAsync(
        IReadOnlyList<Guid> skillIds,
        CancellationToken cancellationToken)
    {
        if (skillIds.Any(id => id == Guid.Empty))
        {
            return "Skill is required.";
        }

        if (skillIds.Distinct().Count() != skillIds.Count)
        {
            return "Duplicate skill IDs are not allowed.";
        }

        if (skillIds.Count == 0)
        {
            return null;
        }

        var existingCount = await _db.Skills
            .CountAsync(s => skillIds.Contains(s.Id), cancellationToken);

        if (existingCount != skillIds.Count)
        {
            return "Skill not found.";
        }

        return null;
    }

    private async Task SyncSkillsAsync(
        Guid technicianId,
        IReadOnlyList<Guid> skillIds,
        CancellationToken cancellationToken)
    {
        var distinctIds = skillIds.Distinct().ToList();

        var current = await _db.TechnicianSkills
            .Where(ts => ts.TechnicianId == technicianId)
            .ToListAsync(cancellationToken);

        var toRemove = current.Where(ts => !distinctIds.Contains(ts.SkillId)).ToList();
        var currentIds = current.Select(ts => ts.SkillId).ToHashSet();
        var toAdd = distinctIds
            .Where(skillId => !currentIds.Contains(skillId))
            .Select(skillId => new TechnicianSkill
            {
                TechnicianId = technicianId,
                SkillId = skillId
            });

        _db.TechnicianSkills.RemoveRange(toRemove);
        _db.TechnicianSkills.AddRange(toAdd);
    }

    private async Task<TechnicianResponse> GetResponseAsync(
        Guid technicianId,
        CancellationToken cancellationToken)
    {
        var technician = await _db.Technicians
            .AsNoTracking()
            .Include(t => t.TechnicianSkills)
            .ThenInclude(ts => ts.Skill)
            .SingleAsync(t => t.Id == technicianId, cancellationToken);

        return ToResponseProjection(technician);
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

    private static TechnicianResponse ToResponseProjection(Technician technician) =>
        new()
        {
            Id = technician.Id,
            DealershipId = technician.DealershipId,
            FirstName = technician.FirstName,
            LastName = technician.LastName,
            IsActive = technician.IsActive,
            Skills = technician.TechnicianSkills
                .OrderBy(ts => ts.Skill.Name)
                .Select(ts => new TechnicianSkillSummary
                {
                    Id = ts.SkillId,
                    Name = ts.Skill.Name
                })
                .ToList()
        };
}
