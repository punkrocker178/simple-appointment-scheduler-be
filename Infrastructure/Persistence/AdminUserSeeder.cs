using Infrastructure.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Persistence;

public static class AdminUserSeeder
{
    public static async Task SeedAsync(
        Infrastructure.Data.ApplicationDbContext db,
        IPasswordHasher<User> passwordHasher,
        IConfiguration configuration,
        ILogger logger,
        CancellationToken cancellationToken = default)
    {
        var email = configuration["AdminSeed:Email"];
        var password = configuration["AdminSeed:Password"];
        var firstName = configuration["AdminSeed:FirstName"] ?? "System";
        var lastName = configuration["AdminSeed:LastName"] ?? "Admin";

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            logger.LogWarning(
                "Admin user seed skipped. Set AdminSeed:Email and AdminSeed:Password (User Secrets in development).");
            return;
        }

        if (await db.Users.AnyAsync(u => u.Email == email, cancellationToken))
        {
            logger.LogInformation("Admin user seed skipped. User {Email} already exists.", email);
            return;
        }

        var admin = new User
        {
            Id = Guid.NewGuid(),
            Email = email.Trim(),
            PasswordHash = passwordHasher.HashPassword(null!, password),
            RoleId = AuthSeedData.RoleIds.Admin,
            FirstName = firstName.Trim(),
            LastName = lastName.Trim(),
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        db.Users.Add(admin);
        await db.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Seeded admin user {Email}.", email);
    }
}
