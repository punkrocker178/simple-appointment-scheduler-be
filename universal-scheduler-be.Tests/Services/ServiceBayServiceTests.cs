using Infrastructure.Data;
using Infrastructure.Entities;
using Infrastructure.ServiceBays.Dtos;
using Infrastructure.Services;
using Microsoft.AspNetCore.Http;
using universal_scheduler_be.Tests.Helpers;

namespace universal_scheduler_be.Tests.Services;

public class ServiceBayServiceTests
{
    private readonly Guid _dealershipId = Guid.Parse("c3000001-0000-4000-8000-000000000001");

    [Fact]
    public async Task GetByDealershipAsync_ReturnsServiceBaysOrderedByName()
    {
        await using var context = AuthTestData.CreateContext();
        SeedDealership(context);
        context.ServiceBays.AddRange(
            CreateServiceBay("Zebra Bay"),
            CreateServiceBay("Alpha Bay"));
        await context.SaveChangesAsync();

        var service = new ServiceBayService(context);

        var result = await service.GetByDealershipAsync(_dealershipId);

        Assert.Equal(StatusCodes.Status200OK, result.StatusCode);
        Assert.NotNull(result.Data);
        Assert.Equal(["Alpha Bay", "Zebra Bay"], result.Data.Select(sb => sb.Name));
    }

    [Fact]
    public async Task GetByDealershipAsync_MissingDealership_ReturnsNotFound()
    {
        await using var context = AuthTestData.CreateContext();
        var service = new ServiceBayService(context);

        var result = await service.GetByDealershipAsync(Guid.NewGuid());

        Assert.Equal(StatusCodes.Status404NotFound, result.StatusCode);
        Assert.Equal("Dealership not found.", result.Error);
    }

    [Fact]
    public async Task CreateAsync_ValidRequest_ReturnsCreatedAndPersists()
    {
        await using var context = AuthTestData.CreateContext();
        SeedDealership(context);
        var service = new ServiceBayService(context);

        var result = await service.CreateAsync(_dealershipId, new CreateServiceBayRequest
        {
            Name = "  Bay 1  "
        });

        Assert.Equal(StatusCodes.Status201Created, result.StatusCode);
        Assert.NotNull(result.Data);
        Assert.Equal("Bay 1", result.Data.Name);
        Assert.True(result.Data.IsActive);

        var saved = context.ServiceBays.Single();
        Assert.Equal("Bay 1", saved.Name);
    }

    [Fact]
    public async Task CreateAsync_MissingDealership_ReturnsNotFound()
    {
        await using var context = AuthTestData.CreateContext();
        var service = new ServiceBayService(context);

        var result = await service.CreateAsync(Guid.NewGuid(), new CreateServiceBayRequest
        {
            Name = "Bay 1"
        });

        Assert.Equal(StatusCodes.Status404NotFound, result.StatusCode);
        Assert.Equal("Dealership not found.", result.Error);
    }

    [Fact]
    public async Task CreateAsync_EmptyName_ReturnsBadRequest()
    {
        await using var context = AuthTestData.CreateContext();
        SeedDealership(context);
        var service = new ServiceBayService(context);

        var result = await service.CreateAsync(_dealershipId, new CreateServiceBayRequest
        {
            Name = "   "
        });

        Assert.Equal(StatusCodes.Status400BadRequest, result.StatusCode);
        Assert.Equal("Name is required.", result.Error);
    }

    [Fact]
    public async Task UpdateAsync_ValidRequest_ReturnsOkAndPersists()
    {
        await using var context = AuthTestData.CreateContext();
        SeedDealership(context);
        var serviceBay = CreateServiceBay("Old Name");
        context.ServiceBays.Add(serviceBay);
        await context.SaveChangesAsync();

        var service = new ServiceBayService(context);

        var result = await service.UpdateAsync(_dealershipId, serviceBay.Id, new UpdateServiceBayRequest
        {
            Name = "  New Name  "
        });

        Assert.Equal(StatusCodes.Status200OK, result.StatusCode);
        Assert.NotNull(result.Data);
        Assert.Equal("New Name", result.Data.Name);
    }

    [Fact]
    public async Task UpdateAsync_MissingServiceBay_ReturnsNotFound()
    {
        await using var context = AuthTestData.CreateContext();
        SeedDealership(context);
        var service = new ServiceBayService(context);

        var result = await service.UpdateAsync(_dealershipId, Guid.NewGuid(), new UpdateServiceBayRequest
        {
            Name = "Bay 1"
        });

        Assert.Equal(StatusCodes.Status404NotFound, result.StatusCode);
        Assert.Equal("Service bay not found.", result.Error);
    }

    [Fact]
    public async Task SoftDeleteAsync_ActiveServiceBay_ReturnsNoContentAndDeactivates()
    {
        await using var context = AuthTestData.CreateContext();
        SeedDealership(context);
        var serviceBay = CreateServiceBay("Bay 1");
        context.ServiceBays.Add(serviceBay);
        await context.SaveChangesAsync();

        var service = new ServiceBayService(context);

        var result = await service.SoftDeleteAsync(_dealershipId, serviceBay.Id);

        Assert.Equal(StatusCodes.Status204NoContent, result.StatusCode);

        var saved = await context.ServiceBays.FindAsync(serviceBay.Id);
        Assert.NotNull(saved);
        Assert.False(saved.IsActive);
    }

    [Fact]
    public async Task SoftDeleteAsync_AlreadyInactive_ReturnsBadRequest()
    {
        await using var context = AuthTestData.CreateContext();
        SeedDealership(context);
        var serviceBay = CreateServiceBay("Bay 1");
        serviceBay.IsActive = false;
        context.ServiceBays.Add(serviceBay);
        await context.SaveChangesAsync();

        var service = new ServiceBayService(context);

        var result = await service.SoftDeleteAsync(_dealershipId, serviceBay.Id);

        Assert.Equal(StatusCodes.Status400BadRequest, result.StatusCode);
        Assert.Equal("Service bay is already inactive.", result.Error);
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

    private ServiceBay CreateServiceBay(string name) =>
        new()
        {
            Id = Guid.NewGuid(),
            DealershipId = _dealershipId,
            Name = name,
            IsActive = true
        };
}
