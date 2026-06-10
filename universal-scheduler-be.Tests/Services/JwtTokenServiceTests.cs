using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Infrastructure.Auth;
using Infrastructure.Entities;
using Infrastructure.Persistence;
using Infrastructure.Services;
using Microsoft.Extensions.Options;
using universal_scheduler_be.Tests.Helpers;

namespace universal_scheduler_be.Tests.Services;

public class JwtTokenServiceTests
{
    [Fact]
    public void CreateToken_ReturnsValidJwtWithExpectedClaims()
    {
        var options = Options.Create(AuthTestData.CreateJwtOptions());
        var service = new JwtTokenService(options);
        var user = CreateUser();

        var response = service.CreateToken(user, ["appointments:read:own"]);

        Assert.Equal(user.Email, response.Email);
        Assert.Equal("User", response.Role);
        Assert.False(string.IsNullOrWhiteSpace(response.Token));
        Assert.True(response.ExpiresAt > DateTime.UtcNow);

        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(response.Token);
        Assert.Equal("test-issuer", jwt.Issuer);
        Assert.Contains(jwt.Audiences, audience => audience == "test-audience");
        Assert.Equal(user.Id.ToString(), jwt.Subject);
        Assert.Contains(jwt.Claims, claim => claim.Type == ClaimTypes.Role && claim.Value == "User");
        Assert.Contains(jwt.Claims, claim => claim.Type == "permission" && claim.Value == "appointments:read:own");
    }

    [Fact]
    public void CreateToken_IncludesAllPermissionsAsClaims()
    {
        var options = Options.Create(AuthTestData.CreateJwtOptions());
        var service = new JwtTokenService(options);
        var user = CreateUser();
        var permissions = new[] { "appointments:read:own", "appointments:write" };

        var response = service.CreateToken(user, permissions);

        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(response.Token);
        var permissionClaims = jwt.Claims
            .Where(claim => claim.Type == "permission")
            .Select(claim => claim.Value)
            .ToArray();

        Assert.Equal(permissions, permissionClaims);
    }

    [Fact]
    public void CreateToken_WhenKeyMissing_ThrowsInvalidOperationException()
    {
        var options = Options.Create(new JwtOptions
        {
            Key = "",
            Issuer = "test-issuer",
            Audience = "test-audience"
        });
        var service = new JwtTokenService(options);

        var exception = Assert.Throws<InvalidOperationException>(() =>
            service.CreateToken(CreateUser(), []));

        Assert.Equal("Jwt:Key is not configured.", exception.Message);
    }

    private static User CreateUser() =>
        new()
        {
            Id = Guid.NewGuid(),
            Email = "user@example.com",
            Role = new Role
            {
                Id = AuthSeedData.RoleIds.User,
                Name = "User"
            }
        };
}
