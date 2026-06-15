using Infrastructure.Data;
using Infrastructure.Entities;
using Infrastructure.ServiceTypes.Dtos;
using Infrastructure.Services;
using Microsoft.AspNetCore.Http;
using universal_scheduler_be.Tests.Helpers;

namespace universal_scheduler_be.Tests.Services;

public class ServiceTypeServiceTests
{
    private readonly Guid _dealershipId = Guid.Parse("c3000001-0000-4000-8000-000000000001");
    private readonly Guid _skillId = Guid.Parse("d4000001-0000-4000-8000-000000000001");

    [Fact]
    public async Task GetByDealershipAsync_ReturnsServiceTypesOrderedByName()
    {
        await using var context = AuthTestData.CreateContext();
        SeedDealershipAndSkill(context);
        context.ServiceTypes.AddRange(
            CreateServiceType("Zebra Service"),
            CreateServiceType("Alpha Service"));
        await context.SaveChangesAsync();

        var service = new ServiceTypeService(context);

        var result = await service.GetByDealershipAsync(_dealershipId);

        Assert.Equal(StatusCodes.Status200OK, result.StatusCode);
        Assert.NotNull(result.Data);
        Assert.Equal(["Alpha Service", "Zebra Service"], result.Data.Select(st => st.Name));
    }

    [Fact]
    public async Task GetByDealershipAsync_MissingDealership_ReturnsNotFound()
    {
        await using var context = AuthTestData.CreateContext();
        var service = new ServiceTypeService(context);

        var result = await service.GetByDealershipAsync(Guid.NewGuid());

        Assert.Equal(StatusCodes.Status404NotFound, result.StatusCode);
        Assert.Equal("Dealership not found.", result.Error);
    }

    [Fact]
    public async Task CreateAsync_ValidRequest_ReturnsCreatedAndPersists()
    {
        await using var context = AuthTestData.CreateContext();
        SeedDealershipAndSkill(context);
        var service = new ServiceTypeService(context);

        var result = await service.CreateAsync(_dealershipId, new CreateServiceTypeRequest
        {
            SkillId = _skillId,
            Name = "  Oil Change  ",
            Description = "  Standard oil change  ",
            DurationMinutes = 30,
            Price = 49.99m
        });

        Assert.Equal(StatusCodes.Status201Created, result.StatusCode);
        Assert.NotNull(result.Data);
        Assert.Equal("Oil Change", result.Data.Name);
        Assert.True(result.Data.IsActive);

        var saved = context.ServiceTypes.Single();
        Assert.Equal("Oil Change", saved.Name);
        Assert.Equal(30, saved.DurationMinutes);
    }

    [Fact]
    public async Task CreateAsync_MissingDealership_ReturnsNotFound()
    {
        await using var context = AuthTestData.CreateContext();
        var service = new ServiceTypeService(context);

        var result = await service.CreateAsync(Guid.NewGuid(), new CreateServiceTypeRequest
        {
            SkillId = _skillId,
            Name = "Oil Change",
            DurationMinutes = 30,
            Price = 49.99m
        });

        Assert.Equal(StatusCodes.Status404NotFound, result.StatusCode);
        Assert.Equal("Dealership not found.", result.Error);
    }

    [Theory]
    [InlineData(0, 49.99, "Duration must be greater than zero.")]
    [InlineData(30, -1, "Price cannot be negative.")]
    public async Task CreateAsync_InvalidDurationOrPrice_ReturnsBadRequest(
        int durationMinutes,
        decimal price,
        string expectedError)
    {
        await using var context = AuthTestData.CreateContext();
        SeedDealershipAndSkill(context);
        var service = new ServiceTypeService(context);

        var result = await service.CreateAsync(_dealershipId, new CreateServiceTypeRequest
        {
            SkillId = _skillId,
            Name = "Oil Change",
            DurationMinutes = durationMinutes,
            Price = price
        });

        Assert.Equal(StatusCodes.Status400BadRequest, result.StatusCode);
        Assert.Equal(expectedError, result.Error);
    }

    [Fact]
    public async Task UpdateAsync_ValidRequest_ReturnsOkAndPersists()
    {
        await using var context = AuthTestData.CreateContext();
        SeedDealershipAndSkill(context);
        var serviceType = CreateServiceType("Old Name");
        context.ServiceTypes.Add(serviceType);
        await context.SaveChangesAsync();

        var service = new ServiceTypeService(context);

        var result = await service.UpdateAsync(_dealershipId, serviceType.Id, new UpdateServiceTypeRequest
        {
            SkillId = _skillId,
            Name = "  New Name  ",
            DurationMinutes = 45,
            Price = 59.99m
        });

        Assert.Equal(StatusCodes.Status200OK, result.StatusCode);
        Assert.NotNull(result.Data);
        Assert.Equal("New Name", result.Data.Name);
        Assert.Equal(45, result.Data.DurationMinutes);
    }

    [Fact]
    public async Task UpdateAsync_MissingServiceType_ReturnsNotFound()
    {
        await using var context = AuthTestData.CreateContext();
        SeedDealershipAndSkill(context);
        var service = new ServiceTypeService(context);

        var result = await service.UpdateAsync(_dealershipId, Guid.NewGuid(), new UpdateServiceTypeRequest
        {
            SkillId = _skillId,
            Name = "Oil Change",
            DurationMinutes = 30,
            Price = 49.99m
        });

        Assert.Equal(StatusCodes.Status404NotFound, result.StatusCode);
        Assert.Equal("Service type not found.", result.Error);
    }

    [Fact]
    public async Task SoftDeleteAsync_ActiveServiceType_ReturnsNoContentAndDeactivates()
    {
        await using var context = AuthTestData.CreateContext();
        SeedDealershipAndSkill(context);
        var serviceType = CreateServiceType("Oil Change");
        context.ServiceTypes.Add(serviceType);
        await context.SaveChangesAsync();

        var service = new ServiceTypeService(context);

        var result = await service.SoftDeleteAsync(_dealershipId, serviceType.Id);

        Assert.Equal(StatusCodes.Status204NoContent, result.StatusCode);

        var saved = await context.ServiceTypes.FindAsync(serviceType.Id);
        Assert.NotNull(saved);
        Assert.False(saved.IsActive);
    }

    [Fact]
    public async Task SoftDeleteAsync_AlreadyInactive_ReturnsBadRequest()
    {
        await using var context = AuthTestData.CreateContext();
        SeedDealershipAndSkill(context);
        var serviceType = CreateServiceType("Oil Change");
        serviceType.IsActive = false;
        context.ServiceTypes.Add(serviceType);
        await context.SaveChangesAsync();

        var service = new ServiceTypeService(context);

        var result = await service.SoftDeleteAsync(_dealershipId, serviceType.Id);

        Assert.Equal(StatusCodes.Status400BadRequest, result.StatusCode);
        Assert.Equal("Service type is already inactive.", result.Error);
    }

    private void SeedDealershipAndSkill(ApplicationDbContext context)
    {
        context.Dealerships.Add(new Dealership
        {
            Id = _dealershipId,
            Name = "Test Motors",
            Address = "123 Main St",
            Phone = "+1-512-555-0100",
            Timezone = "America/Chicago"
        });
        context.Skills.Add(new Skill
        {
            Id = _skillId,
            Name = "General"
        });
        context.SaveChanges();
    }

    private ServiceType CreateServiceType(string name) =>
        new()
        {
            Id = Guid.NewGuid(),
            DealershipId = _dealershipId,
            SkillId = _skillId,
            Name = name,
            DurationMinutes = 30,
            Price = 49.99m,
            IsActive = true
        };
}
