using System.Security.Claims;
using Infrastructure.Auth;
using Infrastructure.Auth.Dtos;
using Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace universal_scheduler_be.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IUserCustomerResolver _userCustomerResolver;

    public AuthController(IAuthService authService, IUserCustomerResolver userCustomerResolver)
    {
        _authService = authService;
        _userCustomerResolver = userCustomerResolver;
    }

    [HttpPost("register")]
    [ProducesResponseType(typeof(RegisterResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Register(
        [FromBody] RegisterRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _authService.RegisterAsync(request, cancellationToken);
        return ToActionResult(result);
    }

    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login(
        [FromBody] LoginRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _authService.LoginAsync(request, cancellationToken);
        return ToActionResult(result);
    }

    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Me(CancellationToken cancellationToken)
    {
        var user = User;
        var userIdValue = user.FindFirstValue(ClaimTypes.NameIdentifier) ?? user.FindFirstValue("sub");
        Guid? customerId = null;

        if (Guid.TryParse(userIdValue, out var userId))
        {
            customerId = await _userCustomerResolver.GetCustomerIdAsync(userId, cancellationToken);
        }

        var claims = user.Claims
            .Select(c => new { c.Type, c.Value })
            .ToArray();

        return Ok(new
        {
            UserId = userIdValue,
            Email = user.FindFirstValue(ClaimTypes.Email) ?? user.FindFirstValue("email"),
            Role = user.FindFirstValue(ClaimTypes.Role),
            CustomerId = customerId,
            Permissions = user.FindAll("permission").Select(c => c.Value).ToArray(),
            Claims = claims
        });
    }

    private IActionResult ToActionResult(AuthResult result)
    {
        if (result.Response is not null)
        {
            return StatusCode(result.StatusCode, result.Response);
        }

        return Problem(
            detail: result.Error,
            statusCode: result.StatusCode,
            title: result.StatusCode switch
            {
                StatusCodes.Status409Conflict => "Conflict",
                StatusCodes.Status401Unauthorized => "Unauthorized",
                _ => "Bad Request"
            });
    }
}
