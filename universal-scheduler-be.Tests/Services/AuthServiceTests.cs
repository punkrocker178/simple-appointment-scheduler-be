using Infrastructure.Auth.Dtos;
using Infrastructure.Entities;
using Infrastructure.Persistence;
using Infrastructure.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using universal_scheduler_be.Tests.Helpers;

namespace universal_scheduler_be.Tests.Services;

public class AuthServiceTests
{
    private const string ValidPassword = "password123";

    [Fact]
    public async Task RegisterAsync_ValidRequest_ReturnsCreatedWithoutToken()
    {
        await using var context = AuthTestData.CreateContext();
        var service = CreateService(context);

        var result = await service.RegisterAsync(new RegisterRequest
        {
            Email = "newuser@example.com",
            Password = ValidPassword,
            FirstName = "Jane",
            LastName = "Doe"
        });

        Assert.Equal(StatusCodes.Status201Created, result.StatusCode);
        var response = Assert.IsType<RegisterResponse>(result.Response);
        Assert.Equal("newuser@example.com", response.Email);
        Assert.Equal("User", response.Role);

        var savedUser = context.Users.Single(user => user.Email == "newuser@example.com");
        Assert.Equal("Jane", savedUser.FirstName);
        Assert.Equal("Doe", savedUser.LastName);
        Assert.Equal(AuthSeedData.RoleIds.User, savedUser.RoleId);
        Assert.True(savedUser.IsActive);
    }

    [Fact]
    public async Task RegisterAsync_DuplicateEmail_ReturnsConflict()
    {
        await using var context = AuthTestData.CreateContext();
        var service = CreateService(context);

        var request = new RegisterRequest
        {
            Email = "duplicate@example.com",
            Password = ValidPassword,
            FirstName = "First",
            LastName = "User"
        };

        await service.RegisterAsync(request);

        var result = await service.RegisterAsync(request);

        Assert.Equal(StatusCodes.Status409Conflict, result.StatusCode);
        Assert.Equal("A user with this email already exists.", result.Error);
        Assert.Null(result.Response);
    }

    [Theory]
    [InlineData("", ValidPassword, "Email is required.")]
    [InlineData("user@example.com", "", "Password is required.")]
    [InlineData("user@example.com", "short", "Password must be at least 8 characters.")]
    public async Task RegisterAsync_InvalidCredentials_ReturnsBadRequest(
        string email,
        string password,
        string expectedError)
    {
        await using var context = AuthTestData.CreateContext();
        var service = CreateService(context);

        var result = await service.RegisterAsync(new RegisterRequest
        {
            Email = email,
            Password = password,
            FirstName = "Jane",
            LastName = "Doe"
        });

        Assert.Equal(StatusCodes.Status400BadRequest, result.StatusCode);
        Assert.Equal(expectedError, result.Error);
        Assert.Null(result.Response);
    }

    [Fact]
    public async Task LoginAsync_ValidCredentials_ReturnsSuccessWithToken()
    {
        await using var context = AuthTestData.CreateContext();
        var passwordHasher = new PasswordHasher<User>();
        var email = "login@example.com";
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = email,
            PasswordHash = passwordHasher.HashPassword(null!, ValidPassword),
            RoleId = AuthSeedData.RoleIds.User,
            FirstName = "Login",
            LastName = "User",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var service = CreateService(context);

        var result = await service.LoginAsync(new LoginRequest
        {
            Email = email,
            Password = ValidPassword
        });

        Assert.Equal(StatusCodes.Status200OK, result.StatusCode);
        var response = Assert.IsType<AuthResponse>(result.Response);
        Assert.Equal(email, response.Email);
        Assert.Equal("User", response.Role);
        Assert.False(string.IsNullOrWhiteSpace(response.Token));
    }

    [Fact]
    public async Task LoginAsync_WrongPassword_ReturnsUnauthorized()
    {
        await using var context = AuthTestData.CreateContext();
        var passwordHasher = new PasswordHasher<User>();
        context.Users.Add(new User
        {
            Id = Guid.NewGuid(),
            Email = "login@example.com",
            PasswordHash = passwordHasher.HashPassword(null!, ValidPassword),
            RoleId = AuthSeedData.RoleIds.User,
            FirstName = "Login",
            LastName = "User",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        });
        await context.SaveChangesAsync();

        var service = CreateService(context);

        var result = await service.LoginAsync(new LoginRequest
        {
            Email = "login@example.com",
            Password = "wrong-password"
        });

        Assert.Equal(StatusCodes.Status401Unauthorized, result.StatusCode);
        Assert.Equal("Invalid email or password.", result.Error);
        Assert.Null(result.Response);
    }

    [Fact]
    public async Task LoginAsync_UnknownEmail_ReturnsUnauthorized()
    {
        await using var context = AuthTestData.CreateContext();
        var service = CreateService(context);

        var result = await service.LoginAsync(new LoginRequest
        {
            Email = "missing@example.com",
            Password = ValidPassword
        });

        Assert.Equal(StatusCodes.Status401Unauthorized, result.StatusCode);
        Assert.Equal("Invalid email or password.", result.Error);
    }

    [Fact]
    public async Task LoginAsync_InactiveUser_ReturnsUnauthorized()
    {
        await using var context = AuthTestData.CreateContext();
        var passwordHasher = new PasswordHasher<User>();
        context.Users.Add(new User
        {
            Id = Guid.NewGuid(),
            Email = "inactive@example.com",
            PasswordHash = passwordHasher.HashPassword(null!, ValidPassword),
            RoleId = AuthSeedData.RoleIds.User,
            FirstName = "Inactive",
            LastName = "User",
            IsActive = false,
            CreatedAt = DateTime.UtcNow
        });
        await context.SaveChangesAsync();

        var service = CreateService(context);

        var result = await service.LoginAsync(new LoginRequest
        {
            Email = "inactive@example.com",
            Password = ValidPassword
        });

        Assert.Equal(StatusCodes.Status401Unauthorized, result.StatusCode);
        Assert.Equal("Invalid email or password.", result.Error);
    }

    [Theory]
    [InlineData("", ValidPassword)]
    [InlineData("user@example.com", "")]
    public async Task LoginAsync_MissingCredentials_ReturnsBadRequest(string email, string password)
    {
        await using var context = AuthTestData.CreateContext();
        var service = CreateService(context);

        var result = await service.LoginAsync(new LoginRequest
        {
            Email = email,
            Password = password
        });

        Assert.Equal(StatusCodes.Status400BadRequest, result.StatusCode);
        Assert.Equal("Email and password are required.", result.Error);
    }

    private static AuthService CreateService(Infrastructure.Data.ApplicationDbContext context)
    {
        var passwordHasher = new PasswordHasher<User>();
        var jwtTokenService = new JwtTokenService(Options.Create(AuthTestData.CreateJwtOptions()));
        return new AuthService(context, passwordHasher, jwtTokenService);
    }
}
