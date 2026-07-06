using Infrastructure.Data;
using Infrastructure.Entities;
using Infrastructure.Services;
using Infrastructure.Vehicles.Dtos;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using universal_scheduler_be.Tests.Helpers;

namespace universal_scheduler_be.Tests.Services;

public class VehicleServiceTests
{
    private readonly Guid _customerId = Guid.Parse("a7000001-0000-4000-8000-000000000001");

    [Fact]
    public async Task GetByCustomerAsync_ReturnsVehiclesOrderedByMakeThenModel()
    {
        await using var context = AuthTestData.CreateContext();
        SeedCustomer(context);
        context.Vehicles.AddRange(
            CreateVehicle("Zebra", "Z4"),
            CreateVehicle("Alpha", "A4"));
        await context.SaveChangesAsync();

        var service = new VehicleService(context);

        var result = await service.GetByCustomerAsync(_customerId);

        Assert.Equal(StatusCodes.Status200OK, result.StatusCode);
        Assert.NotNull(result.Data);
        Assert.Equal(["Alpha", "Zebra"], result.Data.Select(v => v.Make));
        Assert.All(result.Data, v => Assert.True(v.CanDelete));
    }

    [Fact]
    public async Task GetByCustomerAsync_VehicleWithAppointments_ReturnsCanDeleteFalse()
    {
        await using var context = AuthTestData.CreateContext();
        var vehicle = await SeedVehicleWithAppointmentAsync(context);
        var service = new VehicleService(context);

        var result = await service.GetByCustomerAsync(_customerId);

        Assert.NotNull(result.Data);
        Assert.False(result.Data.Single(v => v.Id == vehicle.Id).CanDelete);
    }

    [Fact]
    public async Task GetByCustomerAsync_VehicleWithoutAppointments_ReturnsCanDeleteTrue()
    {
        await using var context = AuthTestData.CreateContext();
        SeedCustomer(context);
        var vehicle = CreateVehicle("Toyota", "Camry");
        context.Vehicles.Add(vehicle);
        await context.SaveChangesAsync();

        var service = new VehicleService(context);

        var result = await service.GetByCustomerAsync(_customerId);

        Assert.NotNull(result.Data);
        Assert.True(result.Data.Single().CanDelete);
    }

    [Fact]
    public async Task CreateAsync_ValidRequest_ReturnsCreatedAndPersists()
    {
        await using var context = AuthTestData.CreateContext();
        SeedCustomer(context);
        var service = new VehicleService(context);

        var result = await service.CreateAsync(_customerId, new CreateVehicleRequest
        {
            Make = "  Toyota  ",
            Model = "  Camry  ",
            Year = 2022
        });

        Assert.Equal(StatusCodes.Status201Created, result.StatusCode);
        Assert.NotNull(result.Data);
        Assert.Equal("Toyota", result.Data.Make);
        Assert.Equal(2022, result.Data.Year);
        Assert.True(result.Data.CanDelete);
    }

    [Fact]
    public async Task CreateAsync_InvalidYear_ReturnsBadRequest()
    {
        await using var context = AuthTestData.CreateContext();
        SeedCustomer(context);
        var service = new VehicleService(context);

        var result = await service.CreateAsync(_customerId, new CreateVehicleRequest
        {
            Make = "Toyota",
            Model = "Camry",
            Year = 1800
        });

        Assert.Equal(StatusCodes.Status400BadRequest, result.StatusCode);
        Assert.Contains("Year must be between", result.Error);
    }

    [Fact]
    public async Task DeleteAsync_VehicleWithAppointments_ReturnsConflict()
    {
        await using var context = AuthTestData.CreateContext();
        var vehicle = await SeedVehicleWithAppointmentAsync(context);
        var service = new VehicleService(context);

        var result = await service.DeleteAsync(_customerId, vehicle.Id);

        Assert.Equal(StatusCodes.Status409Conflict, result.StatusCode);
        Assert.Equal("Vehicle has appointments and cannot be deleted.", result.Error);
    }

    [Fact]
    public async Task DeleteAsync_NoAppointments_ReturnsNoContentAndRemoves()
    {
        await using var context = AuthTestData.CreateContext();
        SeedCustomer(context);
        var vehicle = CreateVehicle("Toyota", "Camry");
        context.Vehicles.Add(vehicle);
        await context.SaveChangesAsync();

        var service = new VehicleService(context);

        var result = await service.DeleteAsync(_customerId, vehicle.Id);

        Assert.Equal(StatusCodes.Status204NoContent, result.StatusCode);
        Assert.False(context.Vehicles.Any(v => v.Id == vehicle.Id));
    }

    private void SeedCustomer(ApplicationDbContext context)
    {
        context.Customers.Add(new Customer
        {
            Id = _customerId,
            FirstName = "Jane",
            LastName = "Doe",
            Email = "jane@example.com",
            Phone = "+1-512-555-0200",
            CreatedAt = DateTime.UtcNow
        });
        context.SaveChanges();
    }

    private Vehicle CreateVehicle(string make, string model) =>
        new()
        {
            Id = Guid.NewGuid(),
            CustomerId = _customerId,
            Make = make,
            Model = model,
            Year = 2022
        };

    private async Task<Vehicle> SeedVehicleWithAppointmentAsync(ApplicationDbContext context)
    {
        var dealershipId = Guid.NewGuid();
        var skillId = Guid.NewGuid();
        var serviceTypeId = Guid.NewGuid();
        var technicianId = Guid.NewGuid();
        var serviceBayId = Guid.NewGuid();

        SeedCustomer(context);
        var vehicle = CreateVehicle("Toyota", "Camry");
        context.Vehicles.Add(vehicle);

        context.Dealerships.Add(new Dealership
        {
            Id = dealershipId,
            Name = "Test Motors",
            Address = "123 Main St",
            Phone = "+1-512-555-0100",
            Timezone = "America/Chicago"
        });
        context.Skills.Add(new Skill { Id = skillId, Name = "General" });
        context.ServiceTypes.Add(new ServiceType
        {
            Id = serviceTypeId,
            DealershipId = dealershipId,
            SkillId = skillId,
            Name = "Oil Change",
            DurationMinutes = 30,
            Price = 49.99m,
            IsActive = true
        });
        context.Technicians.Add(new Technician
        {
            Id = technicianId,
            DealershipId = dealershipId,
            FirstName = "Alex",
            LastName = "Rivera",
            IsActive = true
        });
        context.ServiceBays.Add(new ServiceBay
        {
            Id = serviceBayId,
            DealershipId = dealershipId,
            Name = "Bay 1",
            IsActive = true
        });

        var appointment = new Appointment
        {
            Id = Guid.NewGuid(),
            CustomerId = _customerId,
            VehicleId = vehicle.Id,
            TechnicianId = technicianId,
            ServiceBayId = serviceBayId,
            ServiceTypeId = serviceTypeId,
            BookingDate = DateOnly.FromDateTime(DateTime.UtcNow),
            SecondsFromMidnight = 28_800,
            Status = AppointmentStatus.Scheduled
        };
        context.Appointments.Add(appointment);

        await context.SaveChangesAsync();
        return vehicle;
    }
}
