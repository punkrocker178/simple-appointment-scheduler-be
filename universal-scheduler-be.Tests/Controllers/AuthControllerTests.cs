using System.Security.Claims;
using Infrastructure.Auth;
using Infrastructure.Auth.Dtos;
using Infrastructure.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using universal_scheduler_be.Controllers;

namespace universal_scheduler_be.Tests.Controllers;

public class AuthControllerTests
{
    [Fact]
    public async Task Register_ReturnsCreatedWhenServiceSucceeds()
    {
        var response = new RegisterResponse
        {
            Email = "user@example.com",
            Role = "User"
        };
        var authService = new Mock<IAuthService>();
        authService
            .Setup(service => service.RegisterAsync(It.IsAny<RegisterRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(AuthResult.Created(response));

        var controller = new AuthController(authService.Object, Mock.Of<IUserCustomerResolver>());
        var request = new RegisterRequest
        {
            Email = "user@example.com",
            Password = "password123",
            FirstName = "Jane",
            LastName = "Doe"
        };

        var actionResult = await controller.Register(request, CancellationToken.None);

        var objectResult = Assert.IsType<ObjectResult>(actionResult);
        Assert.Equal(StatusCodes.Status201Created, objectResult.StatusCode);
        Assert.Same(response, objectResult.Value);
    }

    [Fact]
    public async Task Login_ReturnsOkWhenServiceSucceeds()
    {
        var response = new AuthResponse
        {
            Token = "jwt-token",
            Email = "user@example.com",
            Role = "User",
            ExpiresAt = DateTime.UtcNow.AddHours(1)
        };
        var authService = new Mock<IAuthService>();
        authService
            .Setup(service => service.LoginAsync(It.IsAny<LoginRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(AuthResult.Success(response));

        var controller = new AuthController(authService.Object, Mock.Of<IUserCustomerResolver>());

        var actionResult = await controller.Login(
            new LoginRequest { Email = "user@example.com", Password = "password123" },
            CancellationToken.None);

        var objectResult = Assert.IsType<ObjectResult>(actionResult);
        Assert.Equal(StatusCodes.Status200OK, objectResult.StatusCode);
        Assert.Same(response, objectResult.Value);
    }

    [Fact]
    public async Task Register_ReturnsProblemDetailsWhenServiceFails()
    {
        var authService = new Mock<IAuthService>();
        authService
            .Setup(service => service.RegisterAsync(It.IsAny<RegisterRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(AuthResult.Conflict("A user with this email already exists."));

        var controller = new AuthController(authService.Object, Mock.Of<IUserCustomerResolver>());

        var actionResult = await controller.Register(new RegisterRequest(), CancellationToken.None);

        var problemResult = Assert.IsType<ObjectResult>(actionResult);
        Assert.Equal(StatusCodes.Status409Conflict, problemResult.StatusCode);
        var problem = Assert.IsType<ProblemDetails>(problemResult.Value);
        Assert.Equal("Conflict", problem.Title);
        Assert.Equal("A user with this email already exists.", problem.Detail);
    }

    [Fact]
    public async Task Me_ReturnsClaimsFromAuthenticatedUser()
    {
        var userId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(ClaimTypes.Email, "user@example.com"),
            new Claim(ClaimTypes.Role, "User"),
            new Claim("permission", "appointments:read:own"),
            new Claim("permission", "appointments:write")
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var userCustomerResolver = new Mock<IUserCustomerResolver>();
        userCustomerResolver
            .Setup(resolver => resolver.GetCustomerIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(customerId);
        var controller = new AuthController(Mock.Of<IAuthService>(), userCustomerResolver.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(identity)
                }
            }
        };

        var actionResult = await controller.Me(CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(actionResult);
        Assert.NotNull(okResult.Value);

        var payloadType = okResult.Value.GetType();
        Assert.Equal(userId.ToString(), payloadType.GetProperty("UserId")?.GetValue(okResult.Value));
        Assert.Equal("user@example.com", payloadType.GetProperty("Email")?.GetValue(okResult.Value));
        Assert.Equal("User", payloadType.GetProperty("Role")?.GetValue(okResult.Value));
        Assert.Equal(customerId, payloadType.GetProperty("CustomerId")?.GetValue(okResult.Value));

        var permissions = Assert.IsType<string[]>(payloadType.GetProperty("Permissions")?.GetValue(okResult.Value));
        Assert.Equal(["appointments:read:own", "appointments:write"], permissions);
    }
}
