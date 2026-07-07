using Infrastructure.Appointments.Dtos;
using Infrastructure.Auth;
using Infrastructure.Common;
using Infrastructure.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using universal_scheduler_be.Controllers;

namespace universal_scheduler_be.Tests.Controllers;

public class AppointmentControllerTests
{
    private static readonly AppointmentCallerContext Caller = new(
        Guid.NewGuid(),
        "Staff",
        null,
        CanReadAllAppointments: true,
        CanReadOwnAppointments: false);

    private static readonly AppointmentResponse SampleResponse = new()
    {
        Id = Guid.Parse("a1000001-0000-4000-8000-000000000099"),
        CustomerId = Guid.Parse("a7000001-0000-4000-8000-000000000001"),
        VehicleId = Guid.Parse("b8000001-0000-4000-8000-000000000001"),
        ServiceTypeId = Guid.Parse("c9000001-0000-4000-8000-000000000001"),
        TechnicianId = Guid.Parse("e5000001-0000-4000-8000-000000000001"),
        ServiceBayId = Guid.Parse("f6000001-0000-4000-8000-000000000001"),
        BookingDate = new DateOnly(2026, 6, 17),
        SecondsFromMidnight = 28_800,
        DurationMinutes = 60,
        Status = Infrastructure.Entities.AppointmentStatus.Scheduled
    };

    [Fact]
    public async Task Create_ReturnsCreatedAtActionWhenServiceSucceeds()
    {
        var appointmentService = new Mock<IAppointmentService>();
        appointmentService
            .Setup(service => service.CreateAsync(
                It.IsAny<CreateAppointmentRequest>(),
                It.IsAny<AppointmentCallerContext>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(ServiceResult<AppointmentResponse>.Created(SampleResponse));

        var callerResolver = new Mock<IAppointmentCallerResolver>();
        callerResolver
            .Setup(resolver => resolver.ResolveAsync(It.IsAny<System.Security.Claims.ClaimsPrincipal>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Caller);

        var controller = new AppointmentController(appointmentService.Object, callerResolver.Object);

        var actionResult = await controller.Create(new CreateAppointmentRequest(), CancellationToken.None);

        var createdResult = Assert.IsType<CreatedAtActionResult>(actionResult);
        Assert.Equal(nameof(AppointmentController.GetById), createdResult.ActionName);
        Assert.Same(SampleResponse, createdResult.Value);
    }

    [Fact]
    public async Task Create_ReturnsConflictWhenServiceFails()
    {
        var appointmentService = new Mock<IAppointmentService>();
        appointmentService
            .Setup(service => service.CreateAsync(
                It.IsAny<CreateAppointmentRequest>(),
                It.IsAny<AppointmentCallerContext>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(ServiceResult<AppointmentResponse>.Conflict("The requested time slot is no longer available."));

        var callerResolver = new Mock<IAppointmentCallerResolver>();
        callerResolver
            .Setup(resolver => resolver.ResolveAsync(It.IsAny<System.Security.Claims.ClaimsPrincipal>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Caller);

        var controller = new AppointmentController(appointmentService.Object, callerResolver.Object);

        var actionResult = await controller.Create(new CreateAppointmentRequest(), CancellationToken.None);

        var problemResult = Assert.IsType<ObjectResult>(actionResult);
        Assert.Equal(StatusCodes.Status409Conflict, problemResult.StatusCode);
    }

    [Fact]
    public async Task GetById_ReturnsOkWhenServiceSucceeds()
    {
        var appointmentService = new Mock<IAppointmentService>();
        appointmentService
            .Setup(service => service.GetByIdAsync(
                SampleResponse.Id,
                It.IsAny<AppointmentCallerContext>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(ServiceResult<AppointmentResponse>.Ok(SampleResponse));

        var callerResolver = new Mock<IAppointmentCallerResolver>();
        callerResolver
            .Setup(resolver => resolver.ResolveAsync(It.IsAny<System.Security.Claims.ClaimsPrincipal>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Caller);

        var controller = new AppointmentController(appointmentService.Object, callerResolver.Object);

        var actionResult = await controller.GetById(SampleResponse.Id, CancellationToken.None);

        var objectResult = Assert.IsType<ObjectResult>(actionResult);
        Assert.Equal(StatusCodes.Status200OK, objectResult.StatusCode);
    }

    [Fact]
    public async Task GetByDealershipAndDate_ReturnsBadRequestForInvalidDate()
    {
        var appointmentService = new Mock<IAppointmentService>();
        var controller = new AppointmentController(
            appointmentService.Object,
            Mock.Of<IAppointmentCallerResolver>());

        var actionResult = await controller.GetByDealershipAndDate(
            Guid.NewGuid(),
            "not-a-date",
            CancellationToken.None);

        var problemResult = Assert.IsType<ObjectResult>(actionResult);
        Assert.Equal(StatusCodes.Status400BadRequest, problemResult.StatusCode);
    }
}
