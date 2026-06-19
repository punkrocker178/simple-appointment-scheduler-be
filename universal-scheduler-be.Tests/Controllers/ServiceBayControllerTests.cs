using Infrastructure.Common;
using Infrastructure.ServiceBays.Dtos;
using Infrastructure.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using universal_scheduler_be.Controllers;

namespace universal_scheduler_be.Tests.Controllers;

public class ServiceBayControllerTests
{
    private static readonly Guid DealershipId = Guid.Parse("c3000001-0000-4000-8000-000000000001");

    private static readonly ServiceBayResponse SampleResponse = new()
    {
        Id = Guid.Parse("f6000001-0000-4000-8000-000000000001"),
        DealershipId = DealershipId,
        Name = "Bay 1",
        IsActive = true
    };

    [Fact]
    public async Task GetByDealership_ReturnsOkWhenServiceSucceeds()
    {
        var serviceBays = new List<ServiceBayResponse> { SampleResponse };
        var serviceBayService = new Mock<IServiceBayService>();
        serviceBayService
            .Setup(service => service.GetByDealershipAsync(DealershipId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ServiceResult<IReadOnlyList<ServiceBayResponse>>.Ok(serviceBays));

        var controller = new ServiceBayController(serviceBayService.Object);

        var actionResult = await controller.GetByDealership(DealershipId, CancellationToken.None);

        var objectResult = Assert.IsType<ObjectResult>(actionResult);
        Assert.Equal(StatusCodes.Status200OK, objectResult.StatusCode);
        Assert.Same(serviceBays, objectResult.Value);
    }

    [Fact]
    public async Task Create_ReturnsCreatedAtActionWhenServiceSucceeds()
    {
        var serviceBayService = new Mock<IServiceBayService>();
        serviceBayService
            .Setup(service => service.CreateAsync(
                DealershipId,
                It.IsAny<CreateServiceBayRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(ServiceResult<ServiceBayResponse>.Created(SampleResponse));

        var controller = new ServiceBayController(serviceBayService.Object);

        var actionResult = await controller.Create(
            DealershipId,
            new CreateServiceBayRequest(),
            CancellationToken.None);

        var createdResult = Assert.IsType<CreatedAtActionResult>(actionResult);
        Assert.Equal(StatusCodes.Status201Created, createdResult.StatusCode);
        Assert.Equal(nameof(ServiceBayController.GetByDealership), createdResult.ActionName);
        Assert.Equal(DealershipId, createdResult.RouteValues?["dealershipId"]);
    }

    [Fact]
    public async Task SoftDelete_ReturnsNoContentWhenServiceSucceeds()
    {
        var serviceBayService = new Mock<IServiceBayService>();
        serviceBayService
            .Setup(service => service.SoftDeleteAsync(
                DealershipId,
                SampleResponse.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(ServiceResult<object>.NoContent());

        var controller = new ServiceBayController(serviceBayService.Object);

        var actionResult = await controller.SoftDelete(
            DealershipId,
            SampleResponse.Id,
            CancellationToken.None);

        Assert.IsType<NoContentResult>(actionResult);
    }
}
