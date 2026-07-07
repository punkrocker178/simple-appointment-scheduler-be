using System.Security.Claims;

namespace Infrastructure.Auth;

public record AppointmentCallerContext(
    Guid UserId,
    string Role,
    Guid? CustomerId,
    bool CanReadAllAppointments,
    bool CanReadOwnAppointments);

public interface IAppointmentCallerResolver
{
    AppointmentCallerContext? Resolve(ClaimsPrincipal principal);
}

public class AppointmentCallerResolver : IAppointmentCallerResolver
{
    public AppointmentCallerContext? Resolve(ClaimsPrincipal principal)
    {
        var userIdValue = principal.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? principal.FindFirstValue("sub");

        if (!Guid.TryParse(userIdValue, out var userId))
        {
            return null;
        }

        var role = principal.FindFirstValue(ClaimTypes.Role) ?? string.Empty;
        var canReadAll = principal.HasClaim("permission", "appointments:read");
        var canReadOwn = principal.HasClaim("permission", "appointments:read:own");

        return new AppointmentCallerContext(
            userId,
            role,
            principal.GetCustomerId(),
            canReadAll,
            canReadOwn);
    }
}
