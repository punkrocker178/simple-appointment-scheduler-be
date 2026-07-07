using Infrastructure.Entities;
using Infrastructure.Me.Dtos;
using Infrastructure.Persistence;
using Infrastructure.Services;
using Infrastructure.Vehicles.Dtos;
using Microsoft.AspNetCore.Http;
using universal_scheduler_be.Tests.Helpers;

namespace universal_scheduler_be.Tests.Services;

public class MeServiceTests
{
    private readonly Guid _userId = Guid.Parse("f1000001-0000-4000-8000-000000000001");
    private readonly Guid _customerId = Guid.Parse("a7000001-0000-4000-8000-000000000001");

    [Fact]
    public async Task GetCustomerAsync_LinkedUser_ReturnsCustomer()
    {
        await using var context = AuthTestData.CreateContext();
        SeedLinkedUser(context);
        var service = CreateService(context);

        var result = await service.GetCustomerAsync(_userId);

        Assert.Equal(StatusCodes.Status200OK, result.StatusCode);
        Assert.NotNull(result.Data);
        Assert.Equal(_customerId, result.Data.Id);
        Assert.Equal("Jane", result.Data.FirstName);
    }

    [Fact]
    public async Task GetCustomerAsync_UnlinkedUser_ReturnsNotFound()
    {
        await using var context = AuthTestData.CreateContext();
        context.Users.Add(new User
        {
            Id = _userId,
            Email = "orphan@example.com",
            PasswordHash = "hash",
            RoleId = AuthSeedData.RoleIds.User,
            FirstName = "Orphan",
            LastName = "User",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        });
        await context.SaveChangesAsync();

        var service = CreateService(context);

        var result = await service.GetCustomerAsync(_userId);

        Assert.Equal(StatusCodes.Status404NotFound, result.StatusCode);
        Assert.Equal("Customer profile not found.", result.Error);
    }

    [Fact]
    public async Task UpdateCustomerAsync_LinkedUser_UpdatesPhone()
    {
        await using var context = AuthTestData.CreateContext();
        SeedLinkedUser(context);
        var service = CreateService(context);

        var result = await service.UpdateCustomerAsync(_userId, new UpdateMeCustomerRequest
        {
            Phone = "+1-555-9999"
        });

        Assert.Equal(StatusCodes.Status200OK, result.StatusCode);
        Assert.Equal("+1-555-9999", result.Data?.Phone);
    }

    [Fact]
    public async Task CreateVehicleAsync_LinkedUser_ReturnsCreated()
    {
        await using var context = AuthTestData.CreateContext();
        SeedLinkedUser(context);
        var service = CreateService(context);

        var result = await service.CreateVehicleAsync(_userId, new CreateVehicleRequest
        {
            Make = "Honda",
            Model = "Civic",
            Year = 2022
        });

        Assert.Equal(StatusCodes.Status201Created, result.StatusCode);
        Assert.Equal("Honda", result.Data?.Make);
        Assert.Equal(_customerId, result.Data?.CustomerId);
    }

    private void SeedLinkedUser(Infrastructure.Data.ApplicationDbContext context)
    {
        context.Customers.Add(new Customer
        {
            Id = _customerId,
            FirstName = "Jane",
            LastName = "Doe",
            Email = "jane@example.com",
            Phone = "+1-555-0100",
            CreatedAt = DateTime.UtcNow
        });
        context.Users.Add(new User
        {
            Id = _userId,
            Email = "jane@example.com",
            PasswordHash = "hash",
            RoleId = AuthSeedData.RoleIds.User,
            FirstName = "Jane",
            LastName = "Doe",
            CustomerId = _customerId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        });
        context.SaveChanges();
    }

    private static MeService CreateService(Infrastructure.Data.ApplicationDbContext context) =>
        new(context, new VehicleService(context), new AppointmentService(
            context,
            new FixedTimeProvider(DateTimeOffset.UtcNow)));
}
