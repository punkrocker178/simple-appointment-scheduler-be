using System.Security.Claims;
using Infrastructure.Auth;
using Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;

namespace universal_scheduler_be.Tests.Auth;

public class ServiceTypeAuthorizationTests
{
    private static IAuthorizationService CreateAuthorizationService()
    {
        var services = new ServiceCollection();
        services.AddAuthorizationCore(options =>
        {
            AuthorizationExtensions.AddPermissionPolicies(options);
        });
        services.AddLogging();
        return services.BuildServiceProvider().GetRequiredService<IAuthorizationService>();
    }

    [Fact]
    public async Task GetByDealershipPolicy_AllowsCustomerWithReadCustomerPermission()
    {
        var authService = CreateAuthorizationService();
        var user = new ClaimsPrincipal(new ClaimsIdentity(
        [
            new Claim("permission", "servicetypes:read:customer")
        ], "TestAuth"));

        var result = await authService.AuthorizeAsync(user, "servicetypes:read:any");

        Assert.True(result.Succeeded);
    }

    [Fact]
    public async Task GetByDealershipPolicy_AllowsAdminWithReadPermission()
    {
        var authService = CreateAuthorizationService();
        var user = new ClaimsPrincipal(new ClaimsIdentity(
        [
            new Claim("permission", "servicetypes:read")
        ], "TestAuth"));

        var result = await authService.AuthorizeAsync(user, "servicetypes:read:any");

        Assert.True(result.Succeeded);
    }

    [Fact]
    public async Task GetByDealershipPolicy_DeniesUserWithoutPermission()
    {
        var authService = CreateAuthorizationService();
        var user = new ClaimsPrincipal(new ClaimsIdentity(
        [
            new Claim("permission", "appointments:read:own")
        ], "TestAuth"));

        var result = await authService.AuthorizeAsync(user, "servicetypes:read:any");

        Assert.False(result.Succeeded);
    }

    [Fact]
    public void AuthSeedData_ContainsCustomerServiceTypePermission()
    {
        var permissions = AuthSeedData.Permissions.ToList();
        var rolePermissions = AuthSeedData.RolePermissions.ToList();

        var permission = permissions.Single(p => p.Name == "servicetypes:read:customer");
        Assert.Equal("View service types as a customer", permission.Description);

        var userRoleId = AuthSeedData.RoleIds.User;
        Assert.Contains(rolePermissions, rp =>
            rp.RoleId == userRoleId && rp.PermissionId == permission.Id);
    }
}
