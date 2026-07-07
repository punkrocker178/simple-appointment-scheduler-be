using System.Security.Claims;

namespace Infrastructure.Auth;

public static class UserClaimsExtensions
{
    public static Guid? GetCustomerId(this ClaimsPrincipal principal)
    {
        var value = principal.FindFirstValue(AuthClaimTypes.CustomerId);
        return Guid.TryParse(value, out var customerId) ? customerId : null;
    }
}
