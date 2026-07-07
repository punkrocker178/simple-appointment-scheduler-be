using System.Security.Claims;
using Infrastructure.Auth;

namespace universal_scheduler_be.Tests.Auth;

public class AppointmentCallerResolverTests
{
    [Fact]
    public void Resolve_ReadsCustomerIdFromJwtClaim()
    {
        var userId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var principal = new ClaimsPrincipal(new ClaimsIdentity(
        [
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(ClaimTypes.Role, "User"),
            new Claim(AuthClaimTypes.CustomerId, customerId.ToString()),
            new Claim("permission", "appointments:read:own"),
            new Claim("permission", "appointments:write")
        ],
        "TestAuth"));

        var resolver = new AppointmentCallerResolver();
        var context = resolver.Resolve(principal);

        Assert.NotNull(context);
        Assert.Equal(userId, context.UserId);
        Assert.Equal(customerId, context.CustomerId);
        Assert.False(context.CanReadAllAppointments);
        Assert.True(context.CanReadOwnAppointments);
    }
}
