using Infrastructure.Common;
using Infrastructure.Dealerships.Dtos;
using Infrastructure.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using universal_scheduler_be.Controllers;

namespace universal_scheduler_be.Tests.Controllers;

public class DealershipControllerTests
{
    private static readonly DealershipResponse SampleResponse = new()
    {
        Id = Guid.Parse("c3000001-0000-4000-8000-000000000001"),
        Name = "Downtown Auto",
        Address = "123 Main St",
        Phone = "+1-512-555-0100",
        Timezone = "America/Chicago"
    };

    [Fact]
    public async Task GetAll_ReturnsOkWhenServiceSucceeds()
    {
        var dealerships = new List<DealershipResponse> { SampleResponse };
        var dealershipService = new Mock<IDealershipService>();
        dealershipService
            .Setup(service => service.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(ServiceResult<IReadOnlyList<DealershipResponse>>.Ok(dealerships));

        var controller = new DealershipController(dealershipService.Object);

        var actionResult = await controller.GetAll(CancellationToken.None);

        var objectResult = Assert.IsType<ObjectResult>(actionResult);
        Assert.Equal(StatusCodes.Status200OK, objectResult.StatusCode);
        Assert.Same(dealerships, objectResult.Value);
    }

    [Fact]
    public async Task GetById_ReturnsOkWhenServiceSucceeds()
    {
        var dealershipService = new Mock<IDealershipService>();
        dealershipService
            .Setup(service => service.GetByIdAsync(SampleResponse.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ServiceResult<DealershipResponse>.Ok(SampleResponse));

        var controller = new DealershipController(dealershipService.Object);

        var actionResult = await controller.GetById(SampleResponse.Id, CancellationToken.None);

        var objectResult = Assert.IsType<ObjectResult>(actionResult);
        Assert.Equal(StatusCodes.Status200OK, objectResult.StatusCode);
        Assert.Same(SampleResponse, objectResult.Value);
    }

    [Fact]
    public async Task GetById_ReturnsNotFoundWhenServiceFails()
    {
        var dealershipService = new Mock<IDealershipService>();
        dealershipService
            .Setup(service => service.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ServiceResult<DealershipResponse>.NotFound("Dealership not found."));

        var controller = new DealershipController(dealershipService.Object);

        var actionResult = await controller.GetById(Guid.NewGuid(), CancellationToken.None);

        var problemResult = Assert.IsType<ObjectResult>(actionResult);
        Assert.Equal(StatusCodes.Status404NotFound, problemResult.StatusCode);
        var problem = Assert.IsType<ProblemDetails>(problemResult.Value);
        Assert.Equal("Not Found", problem.Title);
        Assert.Equal("Dealership not found.", problem.Detail);
    }

    [Fact]
    public async Task Create_ReturnsCreatedAtActionWhenServiceSucceeds()
    {
        var dealershipService = new Mock<IDealershipService>();
        dealershipService
            .Setup(service => service.CreateAsync(It.IsAny<CreateDealershipRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ServiceResult<DealershipResponse>.Created(SampleResponse));

        var controller = new DealershipController(dealershipService.Object);

        var actionResult = await controller.Create(new CreateDealershipRequest(), CancellationToken.None);

        var createdResult = Assert.IsType<CreatedAtActionResult>(actionResult);
        Assert.Equal(StatusCodes.Status201Created, createdResult.StatusCode);
        Assert.Equal(nameof(DealershipController.GetById), createdResult.ActionName);
        Assert.Equal(SampleResponse.Id, createdResult.RouteValues?["id"]);
        Assert.Same(SampleResponse, createdResult.Value);
    }

    [Fact]
    public async Task Create_ReturnsProblemWhenServiceFails()
    {
        var dealershipService = new Mock<IDealershipService>();
        dealershipService
            .Setup(service => service.CreateAsync(It.IsAny<CreateDealershipRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ServiceResult<DealershipResponse>.BadRequest("Name is required."));

        var controller = new DealershipController(dealershipService.Object);

        var actionResult = await controller.Create(new CreateDealershipRequest(), CancellationToken.None);

        var problemResult = Assert.IsType<ObjectResult>(actionResult);
        Assert.Equal(StatusCodes.Status400BadRequest, problemResult.StatusCode);
        var problem = Assert.IsType<ProblemDetails>(problemResult.Value);
        Assert.Equal("Bad Request", problem.Title);
        Assert.Equal("Name is required.", problem.Detail);
    }

    [Fact]
    public async Task Update_ReturnsOkWhenServiceSucceeds()
    {
        var dealershipService = new Mock<IDealershipService>();
        dealershipService
            .Setup(service => service.UpdateAsync(SampleResponse.Id, It.IsAny<UpdateDealershipRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ServiceResult<DealershipResponse>.Ok(SampleResponse));

        var controller = new DealershipController(dealershipService.Object);

        var actionResult = await controller.Update(
            SampleResponse.Id,
            new UpdateDealershipRequest(),
            CancellationToken.None);

        var objectResult = Assert.IsType<ObjectResult>(actionResult);
        Assert.Equal(StatusCodes.Status200OK, objectResult.StatusCode);
        Assert.Same(SampleResponse, objectResult.Value);
    }

    [Fact]
    public async Task Update_ReturnsNotFoundWhenServiceFails()
    {
        var dealershipService = new Mock<IDealershipService>();
        dealershipService
            .Setup(service => service.UpdateAsync(It.IsAny<Guid>(), It.IsAny<UpdateDealershipRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ServiceResult<DealershipResponse>.NotFound("Dealership not found."));

        var controller = new DealershipController(dealershipService.Object);

        var actionResult = await controller.Update(
            Guid.NewGuid(),
            new UpdateDealershipRequest(),
            CancellationToken.None);

        var problemResult = Assert.IsType<ObjectResult>(actionResult);
        Assert.Equal(StatusCodes.Status404NotFound, problemResult.StatusCode);
        var problem = Assert.IsType<ProblemDetails>(problemResult.Value);
        Assert.Equal("Not Found", problem.Title);
        Assert.Equal("Dealership not found.", problem.Detail);
    }
}
