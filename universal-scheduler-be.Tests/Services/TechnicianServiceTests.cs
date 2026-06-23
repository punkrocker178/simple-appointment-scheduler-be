using Infrastructure.Data;
using Infrastructure.Entities;
using Infrastructure.Services;
using Infrastructure.Technicians.Dtos;
using Microsoft.AspNetCore.Http;
using universal_scheduler_be.Tests.Helpers;

namespace universal_scheduler_be.Tests.Services;

public class TechnicianServiceTests
{
    private readonly Guid _dealershipId = Guid.Parse("c3000001-0000-4000-8000-000000000001");
    private readonly Guid _skillId = Guid.Parse("d4000001-0000-4000-8000-000000000001");
    private readonly Guid _otherSkillId = Guid.Parse("d4000002-0000-4000-8000-000000000002");

    [Fact]
    public async Task GetByDealershipAsync_ReturnsTechniciansOrderedByLastNameThenFirstName()
    {
        await using var context = AuthTestData.CreateContext();
        SeedDealership(context);
        context.Technicians.AddRange(
            CreateTechnician("Zoe", "Zimmerman"),
            CreateTechnician("Alex", "Adams"),
            CreateTechnician("Bob", "Adams"));
        await context.SaveChangesAsync();

        var service = new TechnicianService(context);

        var result = await service.GetByDealershipAsync(_dealershipId);

        Assert.Equal(StatusCodes.Status200OK, result.StatusCode);
        Assert.NotNull(result.Data);
        Assert.Equal(
            ["Alex Adams", "Bob Adams", "Zoe Zimmerman"],
            result.Data.Select(t => $"{t.FirstName} {t.LastName}"));
    }

    [Fact]
    public async Task GetByDealershipAsync_MissingDealership_ReturnsNotFound()
    {
        await using var context = AuthTestData.CreateContext();
        var service = new TechnicianService(context);

        var result = await service.GetByDealershipAsync(Guid.NewGuid());

        Assert.Equal(StatusCodes.Status404NotFound, result.StatusCode);
        Assert.Equal("Dealership not found.", result.Error);
    }

    [Fact]
    public async Task CreateAsync_ValidRequest_ReturnsCreatedAndPersists()
    {
        await using var context = AuthTestData.CreateContext();
        SeedDealership(context);
        var service = new TechnicianService(context);

        var result = await service.CreateAsync(_dealershipId, new CreateTechnicianRequest
        {
            FirstName = "  Alex  ",
            LastName = "  Rivera  "
        });

        Assert.Equal(StatusCodes.Status201Created, result.StatusCode);
        Assert.NotNull(result.Data);
        Assert.Equal("Alex", result.Data.FirstName);
        Assert.Equal("Rivera", result.Data.LastName);
        Assert.True(result.Data.IsActive);

        var saved = context.Technicians.Single();
        Assert.Equal("Alex", saved.FirstName);
        Assert.Equal("Rivera", saved.LastName);
    }

    [Fact]
    public async Task CreateAsync_MissingDealership_ReturnsNotFound()
    {
        await using var context = AuthTestData.CreateContext();
        var service = new TechnicianService(context);

        var result = await service.CreateAsync(Guid.NewGuid(), new CreateTechnicianRequest
        {
            FirstName = "Alex",
            LastName = "Rivera"
        });

        Assert.Equal(StatusCodes.Status404NotFound, result.StatusCode);
        Assert.Equal("Dealership not found.", result.Error);
    }

    [Theory]
    [InlineData("", "Rivera", "First name is required.")]
    [InlineData("Alex", "", "Last name is required.")]
    public async Task CreateAsync_MissingName_ReturnsBadRequest(
        string firstName,
        string lastName,
        string expectedError)
    {
        await using var context = AuthTestData.CreateContext();
        SeedDealership(context);
        var service = new TechnicianService(context);

        var result = await service.CreateAsync(_dealershipId, new CreateTechnicianRequest
        {
            FirstName = firstName,
            LastName = lastName
        });

        Assert.Equal(StatusCodes.Status400BadRequest, result.StatusCode);
        Assert.Equal(expectedError, result.Error);
    }

    [Fact]
    public async Task UpdateAsync_ValidRequest_ReturnsOkAndPersists()
    {
        await using var context = AuthTestData.CreateContext();
        SeedDealership(context);
        var technician = CreateTechnician("Old", "Name");
        context.Technicians.Add(technician);
        await context.SaveChangesAsync();

        var service = new TechnicianService(context);

        var result = await service.UpdateAsync(_dealershipId, technician.Id, new UpdateTechnicianRequest
        {
            FirstName = "  New  ",
            LastName = "  Person  "
        });

        Assert.Equal(StatusCodes.Status200OK, result.StatusCode);
        Assert.NotNull(result.Data);
        Assert.Equal("New", result.Data.FirstName);
        Assert.Equal("Person", result.Data.LastName);
    }

    [Fact]
    public async Task UpdateAsync_MissingTechnician_ReturnsNotFound()
    {
        await using var context = AuthTestData.CreateContext();
        SeedDealership(context);
        var service = new TechnicianService(context);

        var result = await service.UpdateAsync(_dealershipId, Guid.NewGuid(), new UpdateTechnicianRequest
        {
            FirstName = "Alex",
            LastName = "Rivera"
        });

        Assert.Equal(StatusCodes.Status404NotFound, result.StatusCode);
        Assert.Equal("Technician not found.", result.Error);
    }

    [Fact]
    public async Task SoftDeleteAsync_ActiveTechnician_ReturnsNoContentAndDeactivates()
    {
        await using var context = AuthTestData.CreateContext();
        SeedDealership(context);
        var technician = CreateTechnician("Alex", "Rivera");
        context.Technicians.Add(technician);
        await context.SaveChangesAsync();

        var service = new TechnicianService(context);

        var result = await service.SoftDeleteAsync(_dealershipId, technician.Id);

        Assert.Equal(StatusCodes.Status204NoContent, result.StatusCode);

        var saved = await context.Technicians.FindAsync(technician.Id);
        Assert.NotNull(saved);
        Assert.False(saved.IsActive);
    }

    [Fact]
    public async Task SoftDeleteAsync_AlreadyInactive_ReturnsBadRequest()
    {
        await using var context = AuthTestData.CreateContext();
        SeedDealership(context);
        var technician = CreateTechnician("Alex", "Rivera");
        technician.IsActive = false;
        context.Technicians.Add(technician);
        await context.SaveChangesAsync();

        var service = new TechnicianService(context);

        var result = await service.SoftDeleteAsync(_dealershipId, technician.Id);

        Assert.Equal(StatusCodes.Status400BadRequest, result.StatusCode);
        Assert.Equal("Technician is already inactive.", result.Error);
    }

    [Fact]
    public async Task CreateAsync_WithValidSkillIds_PersistsAssignmentsAndReturnsSkills()
    {
        await using var context = AuthTestData.CreateContext();
        SeedDealership(context);
        SeedSkills(context);
        var service = new TechnicianService(context);

        var result = await service.CreateAsync(_dealershipId, new CreateTechnicianRequest
        {
            FirstName = "Alex",
            LastName = "Rivera",
            SkillIds = [_skillId]
        });

        Assert.Equal(StatusCodes.Status201Created, result.StatusCode);
        Assert.NotNull(result.Data);
        Assert.Single(result.Data.Skills);
        Assert.Equal(_skillId, result.Data.Skills[0].Id);
        Assert.Equal("Oil Change", result.Data.Skills[0].Name);
        Assert.True(context.TechnicianSkills.Any(ts => ts.SkillId == _skillId));
    }

    [Fact]
    public async Task CreateAsync_UnknownSkillId_ReturnsBadRequest()
    {
        await using var context = AuthTestData.CreateContext();
        SeedDealership(context);
        var service = new TechnicianService(context);

        var result = await service.CreateAsync(_dealershipId, new CreateTechnicianRequest
        {
            FirstName = "Alex",
            LastName = "Rivera",
            SkillIds = [Guid.NewGuid()]
        });

        Assert.Equal(StatusCodes.Status400BadRequest, result.StatusCode);
        Assert.Equal("Skill not found.", result.Error);
    }

    [Fact]
    public async Task CreateAsync_DuplicateSkillIds_ReturnsBadRequest()
    {
        await using var context = AuthTestData.CreateContext();
        SeedDealership(context);
        SeedSkills(context);
        var service = new TechnicianService(context);

        var result = await service.CreateAsync(_dealershipId, new CreateTechnicianRequest
        {
            FirstName = "Alex",
            LastName = "Rivera",
            SkillIds = [_skillId, _skillId]
        });

        Assert.Equal(StatusCodes.Status400BadRequest, result.StatusCode);
        Assert.Equal("Duplicate skill IDs are not allowed.", result.Error);
    }

    [Fact]
    public async Task UpdateAsync_WithSkillIds_ReplacesAssignments()
    {
        await using var context = AuthTestData.CreateContext();
        SeedDealership(context);
        SeedSkills(context);
        var technician = CreateTechnician("Alex", "Rivera");
        context.Technicians.Add(technician);
        context.TechnicianSkills.Add(new TechnicianSkill
        {
            TechnicianId = technician.Id,
            SkillId = _skillId
        });
        await context.SaveChangesAsync();

        var service = new TechnicianService(context);

        var result = await service.UpdateAsync(_dealershipId, technician.Id, new UpdateTechnicianRequest
        {
            FirstName = "Alex",
            LastName = "Rivera",
            SkillIds = [_otherSkillId]
        });

        Assert.Equal(StatusCodes.Status200OK, result.StatusCode);
        Assert.NotNull(result.Data);
        Assert.Single(result.Data.Skills);
        Assert.Equal(_otherSkillId, result.Data.Skills[0].Id);
        Assert.False(context.TechnicianSkills.Any(ts =>
            ts.TechnicianId == technician.Id && ts.SkillId == _skillId));
        Assert.True(context.TechnicianSkills.Any(ts =>
            ts.TechnicianId == technician.Id && ts.SkillId == _otherSkillId));
    }

    [Fact]
    public async Task UpdateAsync_WithEmptySkillIds_ClearsAssignments()
    {
        await using var context = AuthTestData.CreateContext();
        SeedDealership(context);
        SeedSkills(context);
        var technician = CreateTechnician("Alex", "Rivera");
        context.Technicians.Add(technician);
        context.TechnicianSkills.Add(new TechnicianSkill
        {
            TechnicianId = technician.Id,
            SkillId = _skillId
        });
        await context.SaveChangesAsync();

        var service = new TechnicianService(context);

        var result = await service.UpdateAsync(_dealershipId, technician.Id, new UpdateTechnicianRequest
        {
            FirstName = "Alex",
            LastName = "Rivera",
            SkillIds = []
        });

        Assert.Equal(StatusCodes.Status200OK, result.StatusCode);
        Assert.NotNull(result.Data);
        Assert.Empty(result.Data.Skills);
        Assert.False(context.TechnicianSkills.Any(ts => ts.TechnicianId == technician.Id));
    }

    [Fact]
    public async Task UpdateAsync_WithoutSkillIds_LeavesAssignmentsUnchanged()
    {
        await using var context = AuthTestData.CreateContext();
        SeedDealership(context);
        SeedSkills(context);
        var technician = CreateTechnician("Alex", "Rivera");
        context.Technicians.Add(technician);
        context.TechnicianSkills.Add(new TechnicianSkill
        {
            TechnicianId = technician.Id,
            SkillId = _skillId
        });
        await context.SaveChangesAsync();

        var service = new TechnicianService(context);

        var result = await service.UpdateAsync(_dealershipId, technician.Id, new UpdateTechnicianRequest
        {
            FirstName = "Alex",
            LastName = "Rivera-Smith"
        });

        Assert.Equal(StatusCodes.Status200OK, result.StatusCode);
        Assert.NotNull(result.Data);
        Assert.Single(result.Data.Skills);
        Assert.Equal(_skillId, result.Data.Skills[0].Id);
        Assert.True(context.TechnicianSkills.Any(ts =>
            ts.TechnicianId == technician.Id && ts.SkillId == _skillId));
    }

    [Fact]
    public async Task GetByDealershipAsync_ReturnsAssignedSkills()
    {
        await using var context = AuthTestData.CreateContext();
        SeedDealership(context);
        SeedSkills(context);
        var technician = CreateTechnician("Alex", "Rivera");
        context.Technicians.Add(technician);
        context.TechnicianSkills.Add(new TechnicianSkill
        {
            TechnicianId = technician.Id,
            SkillId = _skillId
        });
        await context.SaveChangesAsync();

        var service = new TechnicianService(context);

        var result = await service.GetByDealershipAsync(_dealershipId);

        Assert.Equal(StatusCodes.Status200OK, result.StatusCode);
        Assert.NotNull(result.Data);
        var returned = Assert.Single(result.Data);
        Assert.Single(returned.Skills);
        Assert.Equal("Oil Change", returned.Skills[0].Name);
    }

    private void SeedDealership(ApplicationDbContext context)
    {
        context.Dealerships.Add(new Dealership
        {
            Id = _dealershipId,
            Name = "Test Motors",
            Address = "123 Main St",
            Phone = "+1-512-555-0100",
            Timezone = "America/Chicago"
        });
        context.SaveChanges();
    }

    private void SeedSkills(ApplicationDbContext context)
    {
        context.Skills.AddRange(
            new Skill
            {
                Id = _skillId,
                Name = "Oil Change",
                Description = "Standard oil change"
            },
            new Skill
            {
                Id = _otherSkillId,
                Name = "Brake Service",
                Description = "Brake inspection and repair"
            });
        context.SaveChanges();
    }

    private Technician CreateTechnician(string firstName, string lastName) =>
        new()
        {
            Id = Guid.NewGuid(),
            DealershipId = _dealershipId,
            FirstName = firstName,
            LastName = lastName,
            IsActive = true
        };
}
