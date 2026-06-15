using Infrastructure.Common;
using Infrastructure.ServiceTypes.Dtos;
using Infrastructure.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using universal_scheduler_be.Controllers;

namespace universal_scheduler_be.Tests.Controllers;

public class ServiceTypeControllerTests
{
    private static readonly Guid DealershipId = Guid.Parse("c3000001-0000-4000-8000-000000000001");

    private static readonly ServiceTypeResponse SampleResponse = new()
    {
        Id = Guid.Parse("e5000001-0000-4000-8000-000000000001"),
        DealershipId = DealershipId,
        SkillId = Guid.Parse("d4000001-0000-4000-8000-000000000001"),
        Name = "Oil Change",
        DurationMinutes = 30,
        Price = 49.99m,
        IsActive = true
    };

    [Fact]
    public async Task GetByDealership_ReturnsOkWhenServiceSucceeds()
    {
        var serviceTypes = new List<ServiceTypeResponse> { SampleResponse };
        var serviceTypeService = new Mock<IServiceTypeService>();
        serviceTypeService
            .Setup(service => service.GetByDealershipAsync(DealershipId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ServiceResult<IReadOnlyList<ServiceTypeResponse>>.Ok(serviceTypes));

        var controller = new ServiceTypeController(serviceTypeService.Object);

        var actionResult = await controller.GetByDealership(DealershipId, CancellationToken.None);

        var objectResult = Assert.IsType<ObjectResult>(actionResult);
        Assert.Equal(StatusCodes.Status200OK, objectResult.StatusCode);
        Assert.Same(serviceTypes, objectResult.Value);
    }

    [Fact]
    public async Task Create_ReturnsCreatedAtActionWhenServiceSucceeds()
    {
        var serviceTypeService = new Mock<IServiceTypeService>();
        serviceTypeService
            .Setup(service => service.CreateAsync(
                DealershipId,
                It.IsAny<CreateServiceTypeRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(ServiceResult<ServiceTypeResponse>.Created(SampleResponse));

        var controller = new ServiceTypeController(serviceTypeService.Object);

        var actionResult = await controller.Create(
            DealershipId,
            new CreateServiceTypeRequest(),
            CancellationToken.None);

        var createdResult = Assert.IsType<CreatedAtActionResult>(actionResult);
        Assert.Equal(StatusCodes.Status201Created, createdResult.StatusCode);
        Assert.Equal(nameof(ServiceTypeController.GetByDealership), createdResult.ActionName);
        Assert.Equal(DealershipId, createdResult.RouteValues?["dealershipId"]);
    }

    [Fact]
    public async Task SoftDelete_ReturnsNoContentWhenServiceSucceeds()
    {
        var serviceTypeService = new Mock<IServiceTypeService>();
        serviceTypeService
            .Setup(service => service.SoftDeleteAsync(
                DealershipId,
                SampleResponse.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(ServiceResult<object>.NoContent());

        var controller = new ServiceTypeController(serviceTypeService.Object);

        var actionResult = await controller.SoftDelete(
            DealershipId,
            SampleResponse.Id,
            CancellationToken.None);

        Assert.IsType<NoContentResult>(actionResult);
    }
}
