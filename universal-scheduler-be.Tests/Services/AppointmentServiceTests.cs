using Infrastructure.Appointments.Dtos;
using Infrastructure.Auth;
using Infrastructure.Entities;
using Infrastructure.Services;
using Microsoft.AspNetCore.Http;
using universal_scheduler_be.Tests.Helpers;

namespace universal_scheduler_be.Tests.Services;

public class AppointmentServiceTests
{
    private static readonly DateOnly BookingDate = new(2026, 6, 17);
    private static readonly DateTimeOffset FixedNow = new(2026, 6, 17, 8, 0, 0, TimeSpan.Zero);

    private readonly Guid _dealershipId = Guid.Parse("c3000001-0000-4000-8000-000000000001");
    private readonly Guid _skillId = Guid.Parse("d4000001-0000-4000-8000-000000000001");
    private readonly Guid _customerId = Guid.Parse("a7000001-0000-4000-8000-000000000001");
    private readonly Guid _vehicleId = Guid.Parse("b8000001-0000-4000-8000-000000000001");
    private readonly Guid _serviceTypeId = Guid.Parse("c9000001-0000-4000-8000-000000000001");
    private readonly Guid _technicianId = Guid.Parse("e5000001-0000-4000-8000-000000000001");
    private readonly Guid _bayId = Guid.Parse("f6000001-0000-4000-8000-000000000001");

    [Fact]
    public async Task CreateAsync_ValidRequest_ReturnsCreatedAndAssignsResources()
    {
        await using var context = AuthTestData.CreateContext();
        SeedSchedulingGraph(context);
        var service = CreateService(context);

        var result = await service.CreateAsync(new CreateAppointmentRequest
        {
            CustomerId = _customerId,
            VehicleId = _vehicleId,
            ServiceTypeId = _serviceTypeId,
            BookingDate = BookingDate,
            SecondsFromMidnight = 28_800
        }, StaffCaller());

        Assert.Equal(StatusCodes.Status201Created, result.StatusCode);
        Assert.NotNull(result.Data);
        Assert.Equal(_technicianId, result.Data.TechnicianId);
        Assert.Equal(_bayId, result.Data.ServiceBayId);
        Assert.Equal(AppointmentStatus.Scheduled, result.Data.Status);
        Assert.Equal(60, result.Data.DurationMinutes);
    }

    [Fact]
    public async Task CreateAsync_SlotTaken_ReturnsConflict()
    {
        await using var context = AuthTestData.CreateContext();
        SeedSchedulingGraph(context);
        context.Appointments.Add(new Appointment
        {
            Id = Guid.NewGuid(),
            CustomerId = _customerId,
            VehicleId = _vehicleId,
            ServiceTypeId = _serviceTypeId,
            TechnicianId = _technicianId,
            ServiceBayId = _bayId,
            BookingDate = BookingDate,
            SecondsFromMidnight = 28_800,
            Status = AppointmentStatus.Scheduled
        });
        await context.SaveChangesAsync();

        var service = CreateService(context);

        var result = await service.CreateAsync(new CreateAppointmentRequest
        {
            CustomerId = _customerId,
            VehicleId = _vehicleId,
            ServiceTypeId = _serviceTypeId,
            BookingDate = BookingDate,
            SecondsFromMidnight = 28_800
        }, StaffCaller());

        Assert.Equal(StatusCodes.Status409Conflict, result.StatusCode);
        Assert.Equal("The requested time slot is no longer available.", result.Error);
    }

    [Fact]
    public async Task CreateAsync_OutsideBusinessHours_ReturnsBadRequest()
    {
        await using var context = AuthTestData.CreateContext();
        SeedSchedulingGraph(context);
        var service = CreateService(context);

        var result = await service.CreateAsync(new CreateAppointmentRequest
        {
            CustomerId = _customerId,
            VehicleId = _vehicleId,
            ServiceTypeId = _serviceTypeId,
            BookingDate = BookingDate,
            SecondsFromMidnight = 59_400
        }, StaffCaller());

        Assert.Equal(StatusCodes.Status400BadRequest, result.StatusCode);
        Assert.Equal("Requested time is outside business hours.", result.Error);
    }

    [Fact]
    public async Task CreateAsync_VehicleNotOwnedByCustomer_ReturnsBadRequest()
    {
        await using var context = AuthTestData.CreateContext();
        SeedSchedulingGraph(context);
        var service = CreateService(context);

        var result = await service.CreateAsync(new CreateAppointmentRequest
        {
            CustomerId = Guid.NewGuid(),
            VehicleId = _vehicleId,
            ServiceTypeId = _serviceTypeId,
            BookingDate = BookingDate,
            SecondsFromMidnight = 28_800
        }, StaffCaller());

        Assert.Equal(StatusCodes.Status404NotFound, result.StatusCode);
        Assert.Equal("Customer not found.", result.Error);
    }

    [Fact]
    public async Task GetByIdAsync_ExistingAppointment_ReturnsOk()
    {
        await using var context = AuthTestData.CreateContext();
        SeedSchedulingGraph(context);
        var appointmentId = Guid.NewGuid();
        context.Appointments.Add(new Appointment
        {
            Id = appointmentId,
            CustomerId = _customerId,
            VehicleId = _vehicleId,
            ServiceTypeId = _serviceTypeId,
            TechnicianId = _technicianId,
            ServiceBayId = _bayId,
            BookingDate = BookingDate,
            SecondsFromMidnight = 30_600,
            Status = AppointmentStatus.Scheduled
        });
        await context.SaveChangesAsync();

        var service = CreateService(context);

        var result = await service.GetByIdAsync(appointmentId, StaffCaller());

        Assert.Equal(StatusCodes.Status200OK, result.StatusCode);
        Assert.NotNull(result.Data);
        Assert.Equal(30_600, result.Data.SecondsFromMidnight);
    }

    [Fact]
    public async Task GetByCustomerAsync_ReturnsCustomerAppointments()
    {
        await using var context = AuthTestData.CreateContext();
        SeedSchedulingGraph(context);
        context.Appointments.Add(new Appointment
        {
            Id = Guid.NewGuid(),
            CustomerId = _customerId,
            VehicleId = _vehicleId,
            ServiceTypeId = _serviceTypeId,
            TechnicianId = _technicianId,
            ServiceBayId = _bayId,
            BookingDate = BookingDate,
            SecondsFromMidnight = 28_800,
            Status = AppointmentStatus.Scheduled
        });
        await context.SaveChangesAsync();

        var service = CreateService(context);

        var result = await service.GetByCustomerAsync(_customerId);

        Assert.Equal(StatusCodes.Status200OK, result.StatusCode);
        Assert.NotNull(result.Data);
        Assert.Single(result.Data);
    }

    [Fact]
    public async Task GetByDealershipAndDateAsync_ReturnsAppointmentsForDate()
    {
        await using var context = AuthTestData.CreateContext();
        SeedSchedulingGraph(context);
        context.Appointments.Add(new Appointment
        {
            Id = Guid.NewGuid(),
            CustomerId = _customerId,
            VehicleId = _vehicleId,
            ServiceTypeId = _serviceTypeId,
            TechnicianId = _technicianId,
            ServiceBayId = _bayId,
            BookingDate = BookingDate,
            SecondsFromMidnight = 28_800,
            Status = AppointmentStatus.Scheduled
        });
        await context.SaveChangesAsync();

        var service = CreateService(context);

        var result = await service.GetByDealershipAndDateAsync(_dealershipId, BookingDate);

        Assert.Equal(StatusCodes.Status200OK, result.StatusCode);
        Assert.NotNull(result.Data);
        Assert.Single(result.Data);
    }

    [Fact]
    public async Task GetByDealershipAndDateRangeAsync_ReturnsAppointmentsForRange()
    {
        await using var context = AuthTestData.CreateContext();
        SeedSchedulingGraph(context);
        context.Appointments.Add(new Appointment
        {
            Id = Guid.NewGuid(),
            CustomerId = _customerId,
            VehicleId = _vehicleId,
            ServiceTypeId = _serviceTypeId,
            TechnicianId = _technicianId,
            ServiceBayId = _bayId,
            BookingDate = BookingDate,
            SecondsFromMidnight = 28_800,
            Status = AppointmentStatus.Scheduled
        });
        context.Appointments.Add(new Appointment
        {
            Id = Guid.NewGuid(),
            CustomerId = _customerId,
            VehicleId = _vehicleId,
            ServiceTypeId = _serviceTypeId,
            TechnicianId = _technicianId,
            ServiceBayId = _bayId,
            BookingDate = BookingDate.AddDays(1),
            SecondsFromMidnight = 30_600,
            Status = AppointmentStatus.Scheduled
        });
        await context.SaveChangesAsync();

        var service = CreateService(context);
        var result = await service.GetByDealershipAndDateRangeAsync(
            _dealershipId,
            BookingDate,
            BookingDate.AddDays(1));

        Assert.Equal(StatusCodes.Status200OK, result.StatusCode);
        Assert.NotNull(result.Data);
        Assert.Equal(2, result.Data.Count);
    }

    [Fact]
    public async Task GetByDealershipAndDateRangeAsync_ExceedsMaxRange_ReturnsBadRequest()
    {
        await using var context = AuthTestData.CreateContext();
        SeedSchedulingGraph(context);
        var service = CreateService(context);

        var result = await service.GetByDealershipAndDateRangeAsync(
            _dealershipId,
            BookingDate,
            BookingDate.AddDays(15));

        Assert.Equal(StatusCodes.Status400BadRequest, result.StatusCode);
    }

    [Fact]
    public async Task GetByDealershipAndDateRangeAsync_InvalidRange_ReturnsBadRequest()
    {
        await using var context = AuthTestData.CreateContext();
        SeedSchedulingGraph(context);
        var service = CreateService(context);

        var result = await service.GetByDealershipAndDateRangeAsync(
            _dealershipId,
            BookingDate.AddDays(1),
            BookingDate);

        Assert.Equal(StatusCodes.Status400BadRequest, result.StatusCode);
    }

    private AppointmentService CreateService(Infrastructure.Data.ApplicationDbContext context) =>
        new(context, new FixedTimeProvider(FixedNow));

    private void SeedSchedulingGraph(Infrastructure.Data.ApplicationDbContext context)
    {
        context.Dealerships.Add(new Dealership
        {
            Id = _dealershipId,
            Name = "Downtown Auto",
            Address = "123 Main St",
            Phone = "+1-512-555-0100",
            Timezone = "America/Chicago",
            OpenSecondsFromMidnight = 28_800,
            CloseSecondsFromMidnight = 61_200
        });
        context.Skills.Add(new Skill { Id = _skillId, Name = "General" });
        context.Customers.Add(new Customer
        {
            Id = _customerId,
            FirstName = "Jane",
            LastName = "Doe",
            Email = "jane@example.com",
            Phone = "+1-555-0100",
            CreatedAt = FixedNow.UtcDateTime
        });
        context.Vehicles.Add(new Vehicle
        {
            Id = _vehicleId,
            CustomerId = _customerId,
            Make = "Toyota",
            Model = "Camry",
            Year = 2020
        });
        context.ServiceTypes.Add(new ServiceType
        {
            Id = _serviceTypeId,
            DealershipId = _dealershipId,
            SkillId = _skillId,
            Name = "Oil Change",
            DurationMinutes = 60,
            Price = 49.99m,
            IsActive = true
        });
        context.Technicians.Add(new Technician
        {
            Id = _technicianId,
            DealershipId = _dealershipId,
            FirstName = "Alex",
            LastName = "Tech",
            IsActive = true
        });
        context.TechnicianSkills.Add(new TechnicianSkill
        {
            TechnicianId = _technicianId,
            SkillId = _skillId
        });
        context.ServiceBays.Add(new ServiceBay
        {
            Id = _bayId,
            DealershipId = _dealershipId,
            Name = "Bay 1",
            IsActive = true
        });
        context.SaveChanges();
    }

    [Fact]
    public async Task CreateAsync_UserRole_OtherCustomer_ReturnsForbidden()
    {
        await using var context = AuthTestData.CreateContext();
        SeedSchedulingGraph(context);
        var service = CreateService(context);

        var result = await service.CreateAsync(new CreateAppointmentRequest
        {
            CustomerId = Guid.NewGuid(),
            VehicleId = _vehicleId,
            ServiceTypeId = _serviceTypeId,
            BookingDate = BookingDate,
            SecondsFromMidnight = 28_800
        }, UserCaller(_customerId));

        Assert.Equal(StatusCodes.Status403Forbidden, result.StatusCode);
        Assert.Equal(
            "You can only book appointments for your own customer profile.",
            result.Error);
    }

    [Fact]
    public async Task CreateAsync_UserRole_OwnCustomer_ReturnsCreated()
    {
        await using var context = AuthTestData.CreateContext();
        SeedSchedulingGraph(context);
        var service = CreateService(context);

        var result = await service.CreateAsync(new CreateAppointmentRequest
        {
            CustomerId = _customerId,
            VehicleId = _vehicleId,
            ServiceTypeId = _serviceTypeId,
            BookingDate = BookingDate,
            SecondsFromMidnight = 28_800
        }, UserCaller(_customerId));

        Assert.Equal(StatusCodes.Status201Created, result.StatusCode);
    }

    [Fact]
    public async Task GetByIdAsync_UserRole_OwnAppointment_ReturnsOk()
    {
        await using var context = AuthTestData.CreateContext();
        SeedSchedulingGraph(context);
        var appointmentId = Guid.NewGuid();
        context.Appointments.Add(new Appointment
        {
            Id = appointmentId,
            CustomerId = _customerId,
            VehicleId = _vehicleId,
            ServiceTypeId = _serviceTypeId,
            TechnicianId = _technicianId,
            ServiceBayId = _bayId,
            BookingDate = BookingDate,
            SecondsFromMidnight = 30_600,
            Status = AppointmentStatus.Scheduled
        });
        await context.SaveChangesAsync();

        var service = CreateService(context);

        var result = await service.GetByIdAsync(appointmentId, UserCaller(_customerId));

        Assert.Equal(StatusCodes.Status200OK, result.StatusCode);
    }

    [Fact]
    public async Task GetByIdAsync_UserRole_OtherCustomerAppointment_ReturnsForbidden()
    {
        await using var context = AuthTestData.CreateContext();
        SeedSchedulingGraph(context);
        var otherCustomerId = Guid.NewGuid();
        context.Customers.Add(new Customer
        {
            Id = otherCustomerId,
            FirstName = "Other",
            LastName = "Customer",
            Email = "other@example.com",
            Phone = "+1-555-0199",
            CreatedAt = FixedNow.UtcDateTime
        });
        var appointmentId = Guid.NewGuid();
        context.Appointments.Add(new Appointment
        {
            Id = appointmentId,
            CustomerId = otherCustomerId,
            VehicleId = _vehicleId,
            ServiceTypeId = _serviceTypeId,
            TechnicianId = _technicianId,
            ServiceBayId = _bayId,
            BookingDate = BookingDate,
            SecondsFromMidnight = 30_600,
            Status = AppointmentStatus.Scheduled
        });
        await context.SaveChangesAsync();

        var service = CreateService(context);

        var result = await service.GetByIdAsync(appointmentId, UserCaller(_customerId));

        Assert.Equal(StatusCodes.Status403Forbidden, result.StatusCode);
    }

    [Fact]
    public async Task UpdateStatusAsync_Staff_ScheduledToInProgress_ReturnsOk()
    {
        await using var context = AuthTestData.CreateContext();
        SeedSchedulingGraph(context);
        var appointmentId = SeedAppointment(context);

        var service = CreateService(context);

        var result = await service.UpdateStatusAsync(
            appointmentId,
            new UpdateAppointmentStatusRequest { Status = AppointmentStatus.InProgress },
            StaffCaller());

        Assert.Equal(StatusCodes.Status200OK, result.StatusCode);
        Assert.Equal(AppointmentStatus.InProgress, result.Data?.Status);
        Assert.NotNull(result.Data?.StartedAtUtc);
    }

    [Fact]
    public async Task UpdateStatusAsync_Staff_InProgressToCompleted_ReturnsOk()
    {
        await using var context = AuthTestData.CreateContext();
        SeedSchedulingGraph(context);
        var appointmentId = SeedAppointment(context, AppointmentStatus.InProgress);

        var service = CreateService(context);

        var result = await service.UpdateStatusAsync(
            appointmentId,
            new UpdateAppointmentStatusRequest { Status = AppointmentStatus.Completed },
            StaffCaller());

        Assert.Equal(StatusCodes.Status200OK, result.StatusCode);
        Assert.Equal(AppointmentStatus.Completed, result.Data?.Status);
        Assert.NotNull(result.Data?.ClosedAtUtc);
    }

    [Fact]
    public async Task UpdateStatusAsync_Staff_ScheduledToCompleted_ReturnsConflict()
    {
        await using var context = AuthTestData.CreateContext();
        SeedSchedulingGraph(context);
        var appointmentId = SeedAppointment(context);

        var service = CreateService(context);

        var result = await service.UpdateStatusAsync(
            appointmentId,
            new UpdateAppointmentStatusRequest { Status = AppointmentStatus.Completed },
            StaffCaller());

        Assert.Equal(StatusCodes.Status409Conflict, result.StatusCode);
    }

    [Fact]
    public async Task UpdateStatusAsync_UserRole_ReturnsForbidden()
    {
        await using var context = AuthTestData.CreateContext();
        SeedSchedulingGraph(context);
        var appointmentId = SeedAppointment(context);

        var service = CreateService(context);

        var result = await service.UpdateStatusAsync(
            appointmentId,
            new UpdateAppointmentStatusRequest { Status = AppointmentStatus.InProgress },
            UserCaller(_customerId));

        Assert.Equal(StatusCodes.Status403Forbidden, result.StatusCode);
    }

    [Fact]
    public async Task CancelAsync_Customer_OutsideCutoff_ReturnsOk()
    {
        await using var context = AuthTestData.CreateContext();
        SeedSchedulingGraph(context);
        var appointmentId = SeedAppointment(context);
        var service = new AppointmentService(context, new FixedTimeProvider(new DateTimeOffset(2026, 6, 17, 5, 0, 0, TimeSpan.Zero)));

        var result = await service.CancelAsync(
            appointmentId,
            new CancelAppointmentRequest { Reason = "Plans changed" },
            UserCaller(_customerId));

        Assert.Equal(StatusCodes.Status200OK, result.StatusCode);
        Assert.Equal(AppointmentStatus.Cancelled, result.Data?.Status);
        Assert.Equal("Plans changed", result.Data?.CancellationReason);
        Assert.NotNull(result.Data?.ClosedAtUtc);
    }

    [Fact]
    public async Task CancelAsync_Customer_WithinCutoff_ReturnsBadRequest()
    {
        await using var context = AuthTestData.CreateContext();
        SeedSchedulingGraph(context);
        var appointmentId = SeedAppointment(context);
        var service = new AppointmentService(context, new FixedTimeProvider(new DateTimeOffset(2026, 6, 17, 7, 0, 0, TimeSpan.Zero)));

        var result = await service.CancelAsync(
            appointmentId,
            new CancelAppointmentRequest { Reason = "Too late" },
            UserCaller(_customerId));

        Assert.Equal(StatusCodes.Status400BadRequest, result.StatusCode);
    }

    [Fact]
    public async Task CancelAsync_Staff_WithinCutoff_ReturnsOk()
    {
        await using var context = AuthTestData.CreateContext();
        SeedSchedulingGraph(context);
        var appointmentId = SeedAppointment(context);
        var service = new AppointmentService(context, new FixedTimeProvider(new DateTimeOffset(2026, 6, 17, 7, 0, 0, TimeSpan.Zero)));

        var result = await service.CancelAsync(
            appointmentId,
            new CancelAppointmentRequest { Reason = "Customer no-show" },
            StaffCaller());

        Assert.Equal(StatusCodes.Status200OK, result.StatusCode);
        Assert.Equal(AppointmentStatus.Cancelled, result.Data?.Status);
    }

    [Fact]
    public async Task CancelAsync_Customer_InProgress_ReturnsForbidden()
    {
        await using var context = AuthTestData.CreateContext();
        SeedSchedulingGraph(context);
        var appointmentId = SeedAppointment(context, AppointmentStatus.InProgress);

        var service = CreateService(context);

        var result = await service.CancelAsync(
            appointmentId,
            new CancelAppointmentRequest { Reason = "Changed mind" },
            UserCaller(_customerId));

        Assert.Equal(StatusCodes.Status403Forbidden, result.StatusCode);
    }

    [Fact]
    public async Task CancelAsync_EmptyReason_ReturnsBadRequest()
    {
        await using var context = AuthTestData.CreateContext();
        SeedSchedulingGraph(context);
        var appointmentId = SeedAppointment(context);

        var service = CreateService(context);

        var result = await service.CancelAsync(
            appointmentId,
            new CancelAppointmentRequest { Reason = "   " },
            StaffCaller());

        Assert.Equal(StatusCodes.Status400BadRequest, result.StatusCode);
    }

    private Guid SeedAppointment(
        Infrastructure.Data.ApplicationDbContext context,
        AppointmentStatus status = AppointmentStatus.Scheduled)
    {
        var appointmentId = Guid.NewGuid();
        context.Appointments.Add(new Appointment
        {
            Id = appointmentId,
            CustomerId = _customerId,
            VehicleId = _vehicleId,
            ServiceTypeId = _serviceTypeId,
            TechnicianId = _technicianId,
            ServiceBayId = _bayId,
            BookingDate = BookingDate,
            SecondsFromMidnight = 28_800,
            Status = status
        });
        context.SaveChanges();
        return appointmentId;
    }

    private static AppointmentCallerContext StaffCaller() =>
        new(Guid.NewGuid(), "Staff", null, CanReadAllAppointments: true, CanReadOwnAppointments: false);

    private static AppointmentCallerContext UserCaller(Guid customerId) =>
        new(Guid.NewGuid(), "User", customerId, CanReadAllAppointments: false, CanReadOwnAppointments: true);
}
