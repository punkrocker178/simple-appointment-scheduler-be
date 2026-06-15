using Infrastructure.Dealerships.Dtos;
using Infrastructure.Entities;
using Infrastructure.Services;
using Microsoft.AspNetCore.Http;
using universal_scheduler_be.Tests.Helpers;

namespace universal_scheduler_be.Tests.Services;

public class DealershipServiceTests
{
    [Fact]
    public async Task GetAllAsync_ReturnsDealershipsOrderedByName()
    {
        await using var context = AuthTestData.CreateContext();
        context.Dealerships.AddRange(
            CreateDealership("Zebra Motors"),
            CreateDealership("Alpha Auto"));
        await context.SaveChangesAsync();

        var service = new DealershipService(context);

        var result = await service.GetAllAsync();

        Assert.Equal(StatusCodes.Status200OK, result.StatusCode);
        Assert.NotNull(result.Data);
        Assert.Equal(["Alpha Auto", "Zebra Motors"], result.Data.Select(d => d.Name));
    }

    [Fact]
    public async Task GetAllAsync_EmptyDatabase_ReturnsEmptyList()
    {
        await using var context = AuthTestData.CreateContext();
        var service = new DealershipService(context);

        var result = await service.GetAllAsync();

        Assert.Equal(StatusCodes.Status200OK, result.StatusCode);
        Assert.NotNull(result.Data);
        Assert.Empty(result.Data);
    }

    [Fact]
    public async Task GetByIdAsync_ExistingDealership_ReturnsOk()
    {
        await using var context = AuthTestData.CreateContext();
        var dealership = CreateDealership("Downtown Auto");
        context.Dealerships.Add(dealership);
        await context.SaveChangesAsync();

        var service = new DealershipService(context);

        var result = await service.GetByIdAsync(dealership.Id);

        Assert.Equal(StatusCodes.Status200OK, result.StatusCode);
        Assert.NotNull(result.Data);
        Assert.Equal(dealership.Id, result.Data.Id);
        Assert.Equal("Downtown Auto", result.Data.Name);
    }

    [Fact]
    public async Task GetByIdAsync_MissingDealership_ReturnsNotFound()
    {
        await using var context = AuthTestData.CreateContext();
        var service = new DealershipService(context);

        var result = await service.GetByIdAsync(Guid.NewGuid());

        Assert.Equal(StatusCodes.Status404NotFound, result.StatusCode);
        Assert.Equal("Dealership not found.", result.Error);
        Assert.Null(result.Data);
    }

    [Fact]
    public async Task CreateAsync_ValidRequest_ReturnsCreatedAndPersists()
    {
        await using var context = AuthTestData.CreateContext();
        var service = new DealershipService(context);

        var result = await service.CreateAsync(new CreateDealershipRequest
        {
            Name = "  Downtown Auto  ",
            Address = "123 Main St",
            Phone = "+1-512-555-0100",
            Timezone = "America/Chicago"
        });

        Assert.Equal(StatusCodes.Status201Created, result.StatusCode);
        Assert.NotNull(result.Data);
        Assert.Equal("Downtown Auto", result.Data.Name);
        Assert.Equal("123 Main St", result.Data.Address);

        var saved = context.Dealerships.Single();
        Assert.Equal(result.Data.Id, saved.Id);
        Assert.Equal("Downtown Auto", saved.Name);
    }

    [Theory]
    [InlineData("", "123 Main St", "+1-512-555-0100", "America/Chicago", "Name is required.")]
    [InlineData("Downtown Auto", "", "+1-512-555-0100", "America/Chicago", "Address is required.")]
    [InlineData("Downtown Auto", "123 Main St", "", "America/Chicago", "Phone is required.")]
    [InlineData("Downtown Auto", "123 Main St", "+1-512-555-0100", "", "Timezone is required.")]
    public async Task CreateAsync_InvalidRequest_ReturnsBadRequest(
        string name,
        string address,
        string phone,
        string timezone,
        string expectedError)
    {
        await using var context = AuthTestData.CreateContext();
        var service = new DealershipService(context);

        var result = await service.CreateAsync(new CreateDealershipRequest
        {
            Name = name,
            Address = address,
            Phone = phone,
            Timezone = timezone
        });

        Assert.Equal(StatusCodes.Status400BadRequest, result.StatusCode);
        Assert.Equal(expectedError, result.Error);
        Assert.Null(result.Data);
        Assert.Empty(context.Dealerships);
    }

    [Fact]
    public async Task UpdateAsync_ValidRequest_ReturnsOkAndPersists()
    {
        await using var context = AuthTestData.CreateContext();
        var dealership = CreateDealership("Old Name");
        context.Dealerships.Add(dealership);
        await context.SaveChangesAsync();

        var service = new DealershipService(context);

        var result = await service.UpdateAsync(dealership.Id, new UpdateDealershipRequest
        {
            Name = "  New Name  ",
            Address = "456 Congress Ave",
            Phone = "+1-512-555-0199",
            Timezone = "America/Chicago"
        });

        Assert.Equal(StatusCodes.Status200OK, result.StatusCode);
        Assert.NotNull(result.Data);
        Assert.Equal("New Name", result.Data.Name);
        Assert.Equal("456 Congress Ave", result.Data.Address);

        var saved = await context.Dealerships.FindAsync(dealership.Id);
        Assert.NotNull(saved);
        Assert.Equal("New Name", saved.Name);
    }

    [Fact]
    public async Task UpdateAsync_MissingDealership_ReturnsNotFound()
    {
        await using var context = AuthTestData.CreateContext();
        var service = new DealershipService(context);

        var result = await service.UpdateAsync(Guid.NewGuid(), new UpdateDealershipRequest
        {
            Name = "New Name",
            Address = "456 Congress Ave",
            Phone = "+1-512-555-0199",
            Timezone = "America/Chicago"
        });

        Assert.Equal(StatusCodes.Status404NotFound, result.StatusCode);
        Assert.Equal("Dealership not found.", result.Error);
        Assert.Null(result.Data);
    }

    [Theory]
    [InlineData("", "123 Main St", "+1-512-555-0100", "America/Chicago", "Name is required.")]
    [InlineData("Downtown Auto", "", "+1-512-555-0100", "America/Chicago", "Address is required.")]
    [InlineData("Downtown Auto", "123 Main St", "", "America/Chicago", "Phone is required.")]
    [InlineData("Downtown Auto", "123 Main St", "+1-512-555-0100", "   ", "Timezone is required.")]
    public async Task UpdateAsync_InvalidRequest_ReturnsBadRequest(
        string name,
        string address,
        string phone,
        string timezone,
        string expectedError)
    {
        await using var context = AuthTestData.CreateContext();
        var dealership = CreateDealership("Existing");
        context.Dealerships.Add(dealership);
        await context.SaveChangesAsync();

        var service = new DealershipService(context);

        var result = await service.UpdateAsync(dealership.Id, new UpdateDealershipRequest
        {
            Name = name,
            Address = address,
            Phone = phone,
            Timezone = timezone
        });

        Assert.Equal(StatusCodes.Status400BadRequest, result.StatusCode);
        Assert.Equal(expectedError, result.Error);
        Assert.Null(result.Data);
    }

    private static Dealership CreateDealership(string name) =>
        new()
        {
            Id = Guid.NewGuid(),
            Name = name,
            Address = "123 Main St",
            Phone = "+1-512-555-0100",
            Timezone = "America/Chicago"
        };
}
