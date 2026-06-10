using Infrastructure.Auth;
using Infrastructure.Auth.Dtos;
using Infrastructure.Data;
using Infrastructure.Entities;
using Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services;

public class AuthService : IAuthService
{
    private const int MinPasswordLength = 8;

    private readonly ApplicationDbContext _db;
    private readonly IPasswordHasher<User> _passwordHasher;
    private readonly IJwtTokenService _jwtTokenService;

    public AuthService(
        ApplicationDbContext db,
        IPasswordHasher<User> passwordHasher,
        IJwtTokenService jwtTokenService)
    {
        _db = db;
        _passwordHasher = passwordHasher;
        _jwtTokenService = jwtTokenService;
    }

    public async Task<AuthResult> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        var validationError = ValidateCredentials(request.Email, request.Password);
        if (validationError is not null)
        {
            return AuthResult.BadRequest(validationError);
        }

        if (await _db.Users.AnyAsync(u => u.Email == request.Email, cancellationToken))
        {
            return AuthResult.Conflict("A user with this email already exists.");
        }

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = request.Email.Trim(),
            PasswordHash = _passwordHasher.HashPassword(null!, request.Password),
            RoleId = AuthSeedData.RoleIds.User,
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync(cancellationToken);

        var role = await _db.Roles.FindAsync([user.RoleId], cancellationToken);
        if (role is null)
        {
            return AuthResult.BadRequest("Failed to load registered user role.");
        }

        return AuthResult.Created(new RegisterResponse
        {
            Email = user.Email,
            Role = role.Name
        });
    }

    public async Task<AuthResult> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
        {
            return AuthResult.BadRequest("Email and password are required.");
        }

        var user = await _db.Users
            .Include(u => u.Role)
                .ThenInclude(r => r.RolePermissions)
                .ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(u => u.Email == request.Email.Trim(), cancellationToken);

        if (user is null || !user.IsActive)
        {
            return AuthResult.Unauthorized("Invalid email or password.");
        }

        var verification = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
        if (verification == PasswordVerificationResult.Failed)
        {
            return AuthResult.Unauthorized("Invalid email or password.");
        }

        var response = _jwtTokenService.CreateToken(user, GetPermissionNames(user));
        return AuthResult.Success(response);
    }

    private static string? ValidateCredentials(string email, string password)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return "Email is required.";
        }

        if (string.IsNullOrWhiteSpace(password))
        {
            return "Password is required.";
        }

        if (password.Length < MinPasswordLength)
        {
            return $"Password must be at least {MinPasswordLength} characters.";
        }

        return null;
    }

    private static IEnumerable<string> GetPermissionNames(User user) =>
        user.Role.RolePermissions.Select(rp => rp.Permission.Name);
}
