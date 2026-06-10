using Infrastructure.Auth.Dtos;

namespace Infrastructure.Auth;

public class AuthResult
{
    public object? Response { get; init; }
    public string? Error { get; init; }
    public int StatusCode { get; init; }

    public static AuthResult Success(AuthResponse response) =>
        new() { Response = response, StatusCode = StatusCodes.Status200OK };

    public static AuthResult Created(RegisterResponse response) =>
        new() { Response = response, StatusCode = StatusCodes.Status201Created };

    public static AuthResult BadRequest(string error) =>
        new() { Error = error, StatusCode = StatusCodes.Status400BadRequest };

    public static AuthResult Unauthorized(string error) =>
        new() { Error = error, StatusCode = StatusCodes.Status401Unauthorized };

    public static AuthResult Conflict(string error) =>
        new() { Error = error, StatusCode = StatusCodes.Status409Conflict };
}
