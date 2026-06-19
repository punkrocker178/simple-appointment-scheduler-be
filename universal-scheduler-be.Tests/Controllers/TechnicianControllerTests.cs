using Infrastructure.Common;
using Infrastructure.Services;
using Infrastructure.Technicians.Dtos;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using universal_scheduler_be.Controllers;

namespace universal_scheduler_be.Tests.Controllers;

public class TechnicianControllerTests
{
    private static readonly Guid DealershipId = Guid.Parse("c3000001-0000-4000-8000-000000000001");

    private static readonly TechnicianResponse SampleResponse = new()
    {
        Id = Guid.Parse("f7000001-0000-4000-8000-000000000001"),
        DealershipId = DealershipId,
        FirstName = "Alex",
        LastName = "Rivera",
        IsActive = true
    };

    [Fact]
    public async Task GetByDealership_ReturnsOkWhenServiceSucceeds()
    {
        var technicians = new List<TechnicianResponse> { SampleResponse };
        var technicianService = new Mock<ITechnicianService>();
        technicianService
            .Setup(service => service.GetByDealershipAsync(DealershipId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ServiceResult<IReadOnlyList<TechnicianResponse>>.Ok(technicians));

        var controller = new TechnicianController(technicianService.Object);

        var actionResult = await controller.GetByDealership(DealershipId, CancellationToken.None);

        var objectResult = Assert.IsType<ObjectResult>(actionResult);
        Assert.Equal(StatusCodes.Status200OK, objectResult.StatusCode);
        Assert.Same(technicians, objectResult.Value);
    }

    [Fact]
    public async Task Create_ReturnsCreatedAtActionWhenServiceSucceeds()
    {
        var technicianService = new Mock<ITechnicianService>();
        technicianService
            .Setup(service => service.CreateAsync(
                DealershipId,
                It.IsAny<CreateTechnicianRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(ServiceResult<TechnicianResponse>.Created(SampleResponse));

        var controller = new TechnicianController(technicianService.Object);

        var actionResult = await controller.Create(
            DealershipId,
            new CreateTechnicianRequest(),
            CancellationToken.None);

        var createdResult = Assert.IsType<CreatedAtActionResult>(actionResult);
        Assert.Equal(StatusCodes.Status201Created, createdResult.StatusCode);
        Assert.Equal(nameof(TechnicianController.GetByDealership), createdResult.ActionName);
        Assert.Equal(DealershipId, createdResult.RouteValues?["dealershipId"]);
    }

    [Fact]
    public async Task SoftDelete_ReturnsNoContentWhenServiceSucceeds()
    {
        var technicianService = new Mock<ITechnicianService>();
        technicianService
            .Setup(service => service.SoftDeleteAsync(
                DealershipId,
                SampleResponse.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(ServiceResult<object>.NoContent());

        var controller = new TechnicianController(technicianService.Object);

        var actionResult = await controller.SoftDelete(
            DealershipId,
            SampleResponse.Id,
            CancellationToken.None);

        Assert.IsType<NoContentResult>(actionResult);
    }
}
