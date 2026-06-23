using Infrastructure.Customers.Dtos;
using Infrastructure.Entities;
using Infrastructure.Services;
using Microsoft.AspNetCore.Http;
using universal_scheduler_be.Tests.Helpers;

namespace universal_scheduler_be.Tests.Services;

public class CustomerServiceTests
{
    [Fact]
    public async Task GetAllAsync_ReturnsCustomersOrderedByLastNameThenFirstName()
    {
        await using var context = AuthTestData.CreateContext();
        context.Customers.AddRange(
            CreateCustomer("Zebra", "Zane"),
            CreateCustomer("Alpha", "Anna"));
        await context.SaveChangesAsync();

        var service = new CustomerService(context);

        var result = await service.GetAllAsync();

        Assert.Equal(StatusCodes.Status200OK, result.StatusCode);
        Assert.NotNull(result.Data);
        Assert.Equal(["Alpha", "Zebra"], result.Data.Select(c => c.FirstName));
        Assert.Equal(["Anna", "Zane"], result.Data.Select(c => c.LastName));
    }

    [Fact]
    public async Task CreateAsync_ValidRequest_ReturnsCreatedAndPersists()
    {
        await using var context = AuthTestData.CreateContext();
        var service = new CustomerService(context);

        var result = await service.CreateAsync(new CreateCustomerRequest
        {
            FirstName = "  Jane  ",
            LastName = "  Doe  ",
            Email = "  jane@example.com  ",
            Phone = "  +1-512-555-0200  "
        });

        Assert.Equal(StatusCodes.Status201Created, result.StatusCode);
        Assert.NotNull(result.Data);
        Assert.Equal("Jane", result.Data.FirstName);
        Assert.Equal("jane@example.com", result.Data.Email);

        var saved = context.Customers.Single();
        Assert.Equal("Jane", saved.FirstName);
    }

    [Fact]
    public async Task CreateAsync_MissingFirstName_ReturnsBadRequest()
    {
        await using var context = AuthTestData.CreateContext();
        var service = new CustomerService(context);

        var result = await service.CreateAsync(new CreateCustomerRequest
        {
            FirstName = " ",
            LastName = "Doe",
            Email = "jane@example.com",
            Phone = "+1-512-555-0200"
        });

        Assert.Equal(StatusCodes.Status400BadRequest, result.StatusCode);
        Assert.Equal("First name is required.", result.Error);
    }

    [Fact]
    public async Task UpdateAsync_MissingCustomer_ReturnsNotFound()
    {
        await using var context = AuthTestData.CreateContext();
        var service = new CustomerService(context);

        var result = await service.UpdateAsync(Guid.NewGuid(), new UpdateCustomerRequest
        {
            FirstName = "Jane",
            LastName = "Doe",
            Email = "jane@example.com",
            Phone = "+1-512-555-0200"
        });

        Assert.Equal(StatusCodes.Status404NotFound, result.StatusCode);
        Assert.Equal("Customer not found.", result.Error);
    }

    private static Customer CreateCustomer(string firstName, string lastName) =>
        new()
        {
            Id = Guid.NewGuid(),
            FirstName = firstName,
            LastName = lastName,
            Email = $"{firstName.ToLower()}@example.com",
            Phone = "+1-512-555-0200",
            CreatedAt = DateTime.UtcNow
        };
}
