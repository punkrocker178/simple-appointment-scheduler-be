using Infrastructure.Entities;
using Infrastructure.Services;
using Infrastructure.Skills.Dtos;
using Microsoft.AspNetCore.Http;
using universal_scheduler_be.Tests.Helpers;

namespace universal_scheduler_be.Tests.Services;

public class SkillServiceTests
{
    [Fact]
    public async Task GetAllAsync_ReturnsSkillsOrderedByName()
    {
        await using var context = AuthTestData.CreateContext();
        context.Skills.AddRange(
            CreateSkill("Transmission"),
            CreateSkill("Brakes"));
        await context.SaveChangesAsync();

        var service = new SkillService(context);

        var result = await service.GetAllAsync();

        Assert.Equal(StatusCodes.Status200OK, result.StatusCode);
        Assert.NotNull(result.Data);
        Assert.Equal(["Brakes", "Transmission"], result.Data.Select(s => s.Name));
        Assert.All(result.Data, s => Assert.True(s.CanDelete));
    }

    [Fact]
    public async Task GetAllAsync_UnusedSkill_ReturnsCanDeleteTrue()
    {
        await using var context = AuthTestData.CreateContext();
        var skill = CreateSkill("Detailing");
        context.Skills.Add(skill);
        await context.SaveChangesAsync();

        var service = new SkillService(context);

        var result = await service.GetAllAsync();

        Assert.NotNull(result.Data);
        Assert.True(result.Data.Single().CanDelete);
    }

    [Fact]
    public async Task GetAllAsync_SkillLinkedToServiceType_ReturnsCanDeleteFalse()
    {
        await using var context = AuthTestData.CreateContext();
        var dealership = CreateDealership();
        var skill = CreateSkill("Brakes");
        context.Dealerships.Add(dealership);
        context.Skills.Add(skill);
        context.ServiceTypes.Add(new ServiceType
        {
            Id = Guid.NewGuid(),
            DealershipId = dealership.Id,
            SkillId = skill.Id,
            Name = "Brake Service",
            DurationMinutes = 60,
            Price = 99.99m
        });
        await context.SaveChangesAsync();

        var service = new SkillService(context);

        var result = await service.GetAllAsync();

        Assert.NotNull(result.Data);
        Assert.False(result.Data.Single().CanDelete);
    }

    [Fact]
    public async Task GetAllAsync_SkillLinkedToTechnician_ReturnsCanDeleteFalse()
    {
        await using var context = AuthTestData.CreateContext();
        var dealership = CreateDealership();
        var skill = CreateSkill("Brakes");
        var technicianId = Guid.NewGuid();
        context.Dealerships.Add(dealership);
        context.Skills.Add(skill);
        context.Technicians.Add(new Technician
        {
            Id = technicianId,
            DealershipId = dealership.Id,
            FirstName = "Alex",
            LastName = "Rivera",
            IsActive = true
        });
        context.TechnicianSkills.Add(new TechnicianSkill
        {
            TechnicianId = technicianId,
            SkillId = skill.Id
        });
        await context.SaveChangesAsync();

        var service = new SkillService(context);

        var result = await service.GetAllAsync();

        Assert.NotNull(result.Data);
        Assert.False(result.Data.Single().CanDelete);
    }

    [Fact]
    public async Task CreateAsync_ValidRequest_ReturnsCreatedAndPersists()
    {
        await using var context = AuthTestData.CreateContext();
        var service = new SkillService(context);

        var result = await service.CreateAsync(new CreateSkillRequest
        {
            Name = "  Oil Change  ",
            Description = "  Basic oil service  "
        });

        Assert.Equal(StatusCodes.Status201Created, result.StatusCode);
        Assert.NotNull(result.Data);
        Assert.Equal("Oil Change", result.Data.Name);
        Assert.Equal("Basic oil service", result.Data.Description);
        Assert.True(result.Data.CanDelete);

        var saved = context.Skills.Single();
        Assert.Equal("Oil Change", saved.Name);
    }

    [Fact]
    public async Task CreateAsync_EmptyName_ReturnsBadRequest()
    {
        await using var context = AuthTestData.CreateContext();
        var service = new SkillService(context);

        var result = await service.CreateAsync(new CreateSkillRequest { Name = "   " });

        Assert.Equal(StatusCodes.Status400BadRequest, result.StatusCode);
        Assert.Equal("Name is required.", result.Error);
        Assert.Empty(context.Skills);
    }

    [Fact]
    public async Task CreateAsync_DuplicateName_ReturnsConflict()
    {
        await using var context = AuthTestData.CreateContext();
        context.Skills.Add(CreateSkill("Brakes"));
        await context.SaveChangesAsync();

        var service = new SkillService(context);

        var result = await service.CreateAsync(new CreateSkillRequest { Name = "Brakes" });

        Assert.Equal(StatusCodes.Status409Conflict, result.StatusCode);
        Assert.Equal("A skill with this name already exists.", result.Error);
    }

    [Fact]
    public async Task DeleteAsync_ExistingUnusedSkill_ReturnsNoContent()
    {
        await using var context = AuthTestData.CreateContext();
        var skill = CreateSkill("Detailing");
        context.Skills.Add(skill);
        await context.SaveChangesAsync();

        var service = new SkillService(context);

        var result = await service.DeleteAsync(skill.Id);

        Assert.Equal(StatusCodes.Status204NoContent, result.StatusCode);
        Assert.Empty(context.Skills);
    }

    [Fact]
    public async Task DeleteAsync_MissingSkill_ReturnsNotFound()
    {
        await using var context = AuthTestData.CreateContext();
        var service = new SkillService(context);

        var result = await service.DeleteAsync(Guid.NewGuid());

        Assert.Equal(StatusCodes.Status404NotFound, result.StatusCode);
        Assert.Equal("Skill not found.", result.Error);
    }

    [Fact]
    public async Task DeleteAsync_SkillLinkedToServiceType_ReturnsConflict()
    {
        await using var context = AuthTestData.CreateContext();
        var dealership = CreateDealership();
        var skill = CreateSkill("Brakes");
        context.Dealerships.Add(dealership);
        context.Skills.Add(skill);
        context.ServiceTypes.Add(new ServiceType
        {
            Id = Guid.NewGuid(),
            DealershipId = dealership.Id,
            SkillId = skill.Id,
            Name = "Brake Service",
            DurationMinutes = 60,
            Price = 99.99m
        });
        await context.SaveChangesAsync();

        var service = new SkillService(context);

        var result = await service.DeleteAsync(skill.Id);

        Assert.Equal(StatusCodes.Status409Conflict, result.StatusCode);
        Assert.Equal("Skill is in use and cannot be deleted.", result.Error);
        Assert.Single(context.Skills);
    }

    [Fact]
    public async Task DeleteAsync_SkillLinkedToTechnician_ReturnsConflict()
    {
        await using var context = AuthTestData.CreateContext();
        var dealership = CreateDealership();
        var skill = CreateSkill("Brakes");
        var technicianId = Guid.NewGuid();
        context.Dealerships.Add(dealership);
        context.Skills.Add(skill);
        context.Technicians.Add(new Technician
        {
            Id = technicianId,
            DealershipId = dealership.Id,
            FirstName = "Alex",
            LastName = "Rivera",
            IsActive = true
        });
        context.TechnicianSkills.Add(new TechnicianSkill
        {
            TechnicianId = technicianId,
            SkillId = skill.Id
        });
        await context.SaveChangesAsync();

        var service = new SkillService(context);

        var result = await service.DeleteAsync(skill.Id);

        Assert.Equal(StatusCodes.Status409Conflict, result.StatusCode);
        Assert.Equal("Skill is in use and cannot be deleted.", result.Error);
        Assert.Single(context.Skills);
    }

    private static Skill CreateSkill(string name) =>
        new()
        {
            Id = Guid.NewGuid(),
            Name = name
        };

    private static Dealership CreateDealership() =>
        new()
        {
            Id = Guid.NewGuid(),
            Name = "Test Motors",
            Address = "123 Main St",
            Phone = "+1-512-555-0100",
            Timezone = "America/Chicago"
        };
}
