using Infrastructure.Common;
using Infrastructure.Data;
using Infrastructure.Entities;
using Infrastructure.Skills.Dtos;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services;

public class SkillService : ISkillService
{
    private readonly ApplicationDbContext _db;

    public SkillService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<ServiceResult<IReadOnlyList<SkillResponse>>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        var skills = await _db.Skills
            .AsNoTracking()
            .OrderBy(s => s.Name)
            .Select(s => new SkillResponse
            {
                Id = s.Id,
                Name = s.Name,
                Description = s.Description,
                CanDelete = !_db.TechnicianSkills.Any(ts => ts.SkillId == s.Id)
                    && !_db.ServiceTypes.Any(st => st.SkillId == s.Id)
            })
            .ToListAsync(cancellationToken);

        return ServiceResult<IReadOnlyList<SkillResponse>>.Ok(skills);
    }

    public async Task<ServiceResult<SkillResponse>> CreateAsync(
        CreateSkillRequest request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return ServiceResult<SkillResponse>.BadRequest("Name is required.");
        }

        var trimmedName = request.Name.Trim();
        var nameExists = await _db.Skills
            .AnyAsync(s => s.Name == trimmedName, cancellationToken);

        if (nameExists)
        {
            return ServiceResult<SkillResponse>.Conflict("A skill with this name already exists.");
        }

        var skill = new Skill
        {
            Id = Guid.NewGuid(),
            Name = trimmedName,
            Description = string.IsNullOrWhiteSpace(request.Description)
                ? null
                : request.Description.Trim()
        };

        _db.Skills.Add(skill);
        await _db.SaveChangesAsync(cancellationToken);

        return ServiceResult<SkillResponse>.Created(ToResponse(skill, canDelete: true));
    }

    public async Task<ServiceResult<object>> DeleteAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var skill = await _db.Skills.FindAsync([id], cancellationToken);
        if (skill is null)
        {
            return ServiceResult<object>.NotFound("Skill not found.");
        }

        if (await IsSkillInUseAsync(id, cancellationToken))
        {
            return ServiceResult<object>.Conflict("Skill is in use and cannot be deleted.");
        }

        _db.Skills.Remove(skill);
        await _db.SaveChangesAsync(cancellationToken);

        return ServiceResult<object>.NoContent();
    }

    private async Task<bool> IsSkillInUseAsync(Guid skillId, CancellationToken cancellationToken)
    {
        var linkedToTechnician = await _db.TechnicianSkills
            .AnyAsync(ts => ts.SkillId == skillId, cancellationToken);

        if (linkedToTechnician)
        {
            return true;
        }

        return await _db.ServiceTypes.AnyAsync(st => st.SkillId == skillId, cancellationToken);
    }

    private static SkillResponse ToResponse(Skill skill, bool canDelete) =>
        new()
        {
            Id = skill.Id,
            Name = skill.Name,
            Description = skill.Description,
            CanDelete = canDelete
        };
}
