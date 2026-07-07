using Infrastructure.Appointments.Dtos;
using Infrastructure.Common;
using Infrastructure.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using universal_scheduler_be.Controllers;

namespace universal_scheduler_be.Tests.Controllers;

public class AvailabilityControllerTests
{
    private static readonly AvailabilityResponse SampleResponse = new()
    {
        BookingDate = new DateOnly(2026, 6, 17),
        ServiceTypeId = Guid.Parse("c9000001-0000-4000-8000-000000000001"),
        DurationMinutes = 60,
        Slots = [new AvailabilitySlotDto { SecondsFromMidnight = 28_800, Available = true }]
    };

    [Fact]
    public async Task GetAvailability_ReturnsOkWhenServiceSucceeds()
    {
        var availabilityService = new Mock<IAvailabilityService>();
        availabilityService
            .Setup(service => service.GetAvailabilityAsync(
                It.IsAny<Guid>(),
                It.IsAny<Guid>(),
                It.IsAny<DateOnly>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(ServiceResult<AvailabilityResponse>.Ok(SampleResponse));

        var controller = new AvailabilityController(availabilityService.Object);

        var actionResult = await controller.GetAvailability(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "2026-06-17",
            CancellationToken.None);

        var objectResult = Assert.IsType<ObjectResult>(actionResult);
        Assert.Equal(StatusCodes.Status200OK, objectResult.StatusCode);
        Assert.Same(SampleResponse, objectResult.Value);
    }

    [Fact]
    public async Task GetAvailability_ReturnsBadRequestForInvalidDate()
    {
        var availabilityService = new Mock<IAvailabilityService>();
        var controller = new AvailabilityController(availabilityService.Object);

        var actionResult = await controller.GetAvailability(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "06/17/2026",
            CancellationToken.None);

        var problemResult = Assert.IsType<ObjectResult>(actionResult);
        Assert.Equal(StatusCodes.Status400BadRequest, problemResult.StatusCode);
    }
}
