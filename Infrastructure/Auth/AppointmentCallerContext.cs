using System.Security.Claims;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Auth;

public record AppointmentCallerContext(
    Guid UserId,
    string Role,
    Guid? CustomerId,
    bool CanReadAllAppointments,
    bool CanReadOwnAppointments);

public interface IAppointmentCallerResolver
{
    Task<AppointmentCallerContext?> ResolveAsync(
        ClaimsPrincipal principal,
        CancellationToken cancellationToken = default);
}

public class AppointmentCallerResolver : IAppointmentCallerResolver
{
    private readonly ApplicationDbContext _db;

    public AppointmentCallerResolver(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<AppointmentCallerContext?> ResolveAsync(
        ClaimsPrincipal principal,
        CancellationToken cancellationToken = default)
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

        var customerId = await _db.Users
            .AsNoTracking()
            .Where(u => u.Id == userId)
            .Select(u => u.CustomerId)
            .FirstOrDefaultAsync(cancellationToken);

        return new AppointmentCallerContext(
            userId,
            role,
            customerId,
            canReadAll,
            canReadOwn);
    }
}

public interface IUserCustomerResolver
{
    Task<Guid?> GetCustomerIdAsync(Guid userId, CancellationToken cancellationToken = default);
}

public class UserCustomerResolver : IUserCustomerResolver
{
    private readonly ApplicationDbContext _db;

    public UserCustomerResolver(ApplicationDbContext db)
    {
        _db = db;
    }

    public Task<Guid?> GetCustomerIdAsync(Guid userId, CancellationToken cancellationToken = default) =>
        _db.Users
            .AsNoTracking()
            .Where(u => u.Id == userId)
            .Select(u => u.CustomerId)
            .FirstOrDefaultAsync(cancellationToken);
}
