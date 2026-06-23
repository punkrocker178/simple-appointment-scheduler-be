using Infrastructure.Common;
using Infrastructure.Services;
using Infrastructure.Vehicles.Dtos;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using universal_scheduler_be.Controllers;

namespace universal_scheduler_be.Tests.Controllers;

public class VehicleControllerTests
{
    private static readonly Guid CustomerId = Guid.Parse("a7000001-0000-4000-8000-000000000001");

    private static readonly VehicleResponse SampleResponse = new()
    {
        Id = Guid.Parse("b8000001-0000-4000-8000-000000000001"),
        CustomerId = CustomerId,
        Make = "Toyota",
        Model = "Camry",
        Year = 2022
    };

    [Fact]
    public async Task GetByCustomer_ReturnsOkWhenServiceSucceeds()
    {
        var vehicles = new List<VehicleResponse> { SampleResponse };
        var vehicleService = new Mock<IVehicleService>();
        vehicleService
            .Setup(service => service.GetByCustomerAsync(CustomerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ServiceResult<IReadOnlyList<VehicleResponse>>.Ok(vehicles));

        var controller = new VehicleController(vehicleService.Object);

        var actionResult = await controller.GetByCustomer(CustomerId, CancellationToken.None);

        var objectResult = Assert.IsType<ObjectResult>(actionResult);
        Assert.Equal(StatusCodes.Status200OK, objectResult.StatusCode);
        Assert.Same(vehicles, objectResult.Value);
    }

    [Fact]
    public async Task Create_ReturnsCreatedAtActionWhenServiceSucceeds()
    {
        var vehicleService = new Mock<IVehicleService>();
        vehicleService
            .Setup(service => service.CreateAsync(
                CustomerId,
                It.IsAny<CreateVehicleRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(ServiceResult<VehicleResponse>.Created(SampleResponse));

        var controller = new VehicleController(vehicleService.Object);

        var actionResult = await controller.Create(
            CustomerId,
            new CreateVehicleRequest(),
            CancellationToken.None);

        var createdResult = Assert.IsType<CreatedAtActionResult>(actionResult);
        Assert.Equal(StatusCodes.Status201Created, createdResult.StatusCode);
        Assert.Equal(nameof(VehicleController.GetByCustomer), createdResult.ActionName);
        Assert.Equal(CustomerId, createdResult.RouteValues?["customerId"]);
    }

    [Fact]
    public async Task Delete_ReturnsNoContentWhenServiceSucceeds()
    {
        var vehicleService = new Mock<IVehicleService>();
        vehicleService
            .Setup(service => service.DeleteAsync(
                CustomerId,
                SampleResponse.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(ServiceResult<object>.NoContent());

        var controller = new VehicleController(vehicleService.Object);

        var actionResult = await controller.Delete(
            CustomerId,
            SampleResponse.Id,
            CancellationToken.None);

        Assert.IsType<NoContentResult>(actionResult);
    }
}
