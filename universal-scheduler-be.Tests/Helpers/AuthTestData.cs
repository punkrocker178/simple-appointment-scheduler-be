using Infrastructure.Auth;
using Infrastructure.Data;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace universal_scheduler_be.Tests.Helpers;

internal static class AuthTestData
{
    public static ApplicationDbContext CreateContext(string? databaseName = null)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName ?? Guid.NewGuid().ToString())
            .Options;

        var context = new ApplicationDbContext(options);
        SeedAuthData(context);
        return context;
    }

    public static void SeedAuthData(ApplicationDbContext context)
    {
        if (context.Roles.Any())
        {
            return;
        }

        context.Permissions.AddRange(AuthSeedData.Permissions);
        context.Roles.AddRange(AuthSeedData.Roles);
        context.SaveChanges();
        context.RolePermissions.AddRange(AuthSeedData.RolePermissions);
        context.SaveChanges();
    }

    public static JwtOptions CreateJwtOptions() =>
        new()
        {
            Key = "test-signing-key-at-least-32-characters-long",
            Issuer = "test-issuer",
            Audience = "test-audience",
            ExpiryMinutes = 60
        };
}
