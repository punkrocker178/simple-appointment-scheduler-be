using Infrastructure.Common;
using Infrastructure.Customers.Dtos;
using Infrastructure.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using universal_scheduler_be.Controllers;

namespace universal_scheduler_be.Tests.Controllers;

public class CustomerControllerTests
{
    private static readonly CustomerResponse SampleResponse = new()
    {
        Id = Guid.Parse("a7000001-0000-4000-8000-000000000001"),
        FirstName = "Jane",
        LastName = "Doe",
        Email = "jane@example.com",
        Phone = "+1-512-555-0200",
        CreatedAt = DateTime.UtcNow
    };

    [Fact]
    public async Task GetAll_ReturnsOkWhenServiceSucceeds()
    {
        var customers = new List<CustomerResponse> { SampleResponse };
        var customerService = new Mock<ICustomerService>();
        customerService
            .Setup(service => service.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(ServiceResult<IReadOnlyList<CustomerResponse>>.Ok(customers));

        var controller = new CustomerController(customerService.Object);

        var actionResult = await controller.GetAll(CancellationToken.None);

        var objectResult = Assert.IsType<ObjectResult>(actionResult);
        Assert.Equal(StatusCodes.Status200OK, objectResult.StatusCode);
        Assert.Same(customers, objectResult.Value);
    }

    [Fact]
    public async Task Create_ReturnsCreatedAtActionWhenServiceSucceeds()
    {
        var customerService = new Mock<ICustomerService>();
        customerService
            .Setup(service => service.CreateAsync(It.IsAny<CreateCustomerRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ServiceResult<CustomerResponse>.Created(SampleResponse));

        var controller = new CustomerController(customerService.Object);

        var actionResult = await controller.Create(new CreateCustomerRequest(), CancellationToken.None);

        var createdResult = Assert.IsType<CreatedAtActionResult>(actionResult);
        Assert.Equal(StatusCodes.Status201Created, createdResult.StatusCode);
        Assert.Equal(nameof(CustomerController.GetById), createdResult.ActionName);
        Assert.Equal(SampleResponse.Id, createdResult.RouteValues?["id"]);
    }
}
