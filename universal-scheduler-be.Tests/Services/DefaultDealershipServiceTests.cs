using Infrastructure.Entities;
using Infrastructure.Services;
using Microsoft.AspNetCore.Http;
using universal_scheduler_be.Tests.Helpers;

namespace universal_scheduler_be.Tests.Services;

public class DefaultDealershipServiceTests
{
    private readonly Guid _dealershipId = Guid.Parse("c3000001-0000-4000-8000-000000000001");

    [Fact]
    public async Task GetDefaultDealershipAsync_ReturnsFirstDealershipByName()
    {
        await using var context = AuthTestData.CreateContext();
        SeedDealership(context);
        var service = new DefaultDealershipService(context);

        var result = await service.GetDefaultDealershipAsync();

        Assert.Equal(StatusCodes.Status200OK, result.StatusCode);
        Assert.NotNull(result.Data);
        Assert.Equal(_dealershipId, result.Data.DealershipId);
        Assert.Equal("Alpha Motors", result.Data.DealershipName);
    }

    [Fact]
    public async Task GetDefaultDealershipAsync_NoDealership_ReturnsNotFound()
    {
        await using var context = AuthTestData.CreateContext();
        var service = new DefaultDealershipService(context);

        var result = await service.GetDefaultDealershipAsync();

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
        context.SaveChanges();
    }
}
