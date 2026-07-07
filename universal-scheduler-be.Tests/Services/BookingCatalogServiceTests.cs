using Infrastructure.Entities;
using Infrastructure.Services;
using Microsoft.AspNetCore.Http;
using universal_scheduler_be.Tests.Helpers;

namespace universal_scheduler_be.Tests.Services;

public class BookingCatalogServiceTests
{
    private readonly Guid _dealershipId = Guid.Parse("c3000001-0000-4000-8000-000000000001");
    private readonly Guid _skillId = Guid.Parse("d4000001-0000-4000-8000-000000000001");

    [Fact]
    public async Task GetCatalogAsync_ReturnsFirstDealershipWithActiveServiceTypes()
    {
        await using var context = AuthTestData.CreateContext();
        SeedDealership(context);
        var service = new BookingCatalogService(context);

        var result = await service.GetCatalogAsync();

        Assert.Equal(StatusCodes.Status200OK, result.StatusCode);
        Assert.NotNull(result.Data);
        Assert.Equal(_dealershipId, result.Data.DealershipId);
        Assert.Equal("Alpha Motors", result.Data.DealershipName);
        Assert.Single(result.Data.ServiceTypes);
        Assert.Equal("Oil Change", result.Data.ServiceTypes[0].Name);
    }

    [Fact]
    public async Task GetCatalogAsync_NoDealership_ReturnsNotFound()
    {
        await using var context = AuthTestData.CreateContext();
        var service = new BookingCatalogService(context);

        var result = await service.GetCatalogAsync();

        Assert.Equal(StatusCodes.Status404NotFound, result.StatusCode);
    }

    private void SeedDealership(Infrastructure.Data.ApplicationDbContext context)
    {
        context.Dealerships.Add(new Dealership
        {
            Id = _dealershipId,
            Name = "Alpha Motors",
            Address = "1 Main St",
            Phone = "+1-555-0100",
            Timezone = "America/Chicago",
            OpenSecondsFromMidnight = 28_800,
            CloseSecondsFromMidnight = 61_200
        });
        context.Skills.Add(new Skill { Id = _skillId, Name = "General" });
        context.ServiceTypes.Add(new ServiceType
        {
            Id = Guid.NewGuid(),
            DealershipId = _dealershipId,
            SkillId = _skillId,
            Name = "Oil Change",
            Description = "Standard oil change",
            DurationMinutes = 60,
            Price = 49.99m,
            IsActive = true
        });
        context.ServiceTypes.Add(new ServiceType
        {
            Id = Guid.NewGuid(),
            DealershipId = _dealershipId,
            SkillId = _skillId,
            Name = "Retired Service",
            DurationMinutes = 30,
            Price = 19.99m,
            IsActive = false
        });
        context.SaveChanges();
    }
}
