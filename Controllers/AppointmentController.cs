using Infrastructure.Appointments.Dtos;
using Infrastructure.Auth;
using Infrastructure.Common;
using Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace universal_scheduler_be.Controllers;

[ApiController]
[Route("api")]
[Authorize]
public class AppointmentController : ControllerBase
{
    private readonly IAppointmentService _appointmentService;
    private readonly IAppointmentCallerResolver _callerResolver;

    public AppointmentController(
        IAppointmentService appointmentService,
        IAppointmentCallerResolver callerResolver)
    {
        _appointmentService = appointmentService;
        _callerResolver = callerResolver;
    }

    [HttpPost("appointments")]
    [Authorize(Policy = "appointments:write")]
    [ProducesResponseType(typeof(AppointmentResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Create(
        [FromBody] CreateAppointmentRequest request,
        CancellationToken cancellationToken)
    {
        var caller = _callerResolver.Resolve(User);
        if (caller is null)
        {
            return Unauthorized();
        }

        var result = await _appointmentService.CreateAsync(request, caller, cancellationToken);
        if (result.Data is not null && result.StatusCode == StatusCodes.Status201Created)
        {
            return CreatedAtAction(nameof(GetById), new { id = result.Data.Id }, result.Data);
        }

        return ToActionResult(result);
    }

    [HttpGet("appointments/{id:guid}")]
    [Authorize(Policy = "appointments:read:any")]
    [ProducesResponseType(typeof(AppointmentResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var caller = _callerResolver.Resolve(User);
        if (caller is null)
        {
            return Unauthorized();
        }

        var result = await _appointmentService.GetByIdAsync(id, caller, cancellationToken);
        return ToActionResult(result);
    }

    [HttpGet("customers/{customerId:guid}/appointments")]
    [Authorize(Policy = "appointments:read")]
    [ProducesResponseType(typeof(IReadOnlyList<AppointmentResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetByCustomer(Guid customerId, CancellationToken cancellationToken)
    {
        var result = await _appointmentService.GetByCustomerAsync(customerId, cancellationToken);
        return ToActionResult(result);
    }

    [HttpGet("dealerships/{dealershipId:guid}/appointments")]
    [Authorize(Policy = "appointments:read")]
    [ProducesResponseType(typeof(IReadOnlyList<AppointmentResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetByDealershipAndDate(
        Guid dealershipId,
        [FromQuery] string date,
        CancellationToken cancellationToken)
    {
        if (!DateOnly.TryParseExact(date, "yyyy-MM-dd", out var bookingDate))
        {
            return Problem(
                detail: "Date must be in YYYY-MM-DD format.",
                statusCode: StatusCodes.Status400BadRequest,
                title: "Bad Request");
        }

        var result = await _appointmentService.GetByDealershipAndDateAsync(
            dealershipId,
            bookingDate,
            cancellationToken);

        return ToActionResult(result);
    }

    [HttpPatch("appointments/{id:guid}/status")]
    [Authorize(Policy = "appointments:write")]
    [ProducesResponseType(typeof(AppointmentResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> UpdateStatus(
        Guid id,
        [FromBody] UpdateAppointmentStatusRequest request,
        CancellationToken cancellationToken)
    {
        var caller = _callerResolver.Resolve(User);
        if (caller is null)
        {
            return Unauthorized();
        }

        var result = await _appointmentService.UpdateStatusAsync(id, request, caller, cancellationToken);
        return ToActionResult(result);
    }

    [HttpPost("appointments/{id:guid}/cancel")]
    [Authorize(Policy = "appointments:write")]
    [ProducesResponseType(typeof(AppointmentResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Cancel(
        Guid id,
        [FromBody] CancelAppointmentRequest request,
        CancellationToken cancellationToken)
    {
        var caller = _callerResolver.Resolve(User);
        if (caller is null)
        {
            return Unauthorized();
        }

        var result = await _appointmentService.CancelAsync(id, request, caller, cancellationToken);
        return ToActionResult(result);
    }

    private IActionResult ToActionResult<T>(ServiceResult<T> result)
    {
        if (result.Data is not null)
        {
            return StatusCode(result.StatusCode, result.Data);
        }

        return Problem(
            detail: result.Error,
            statusCode: result.StatusCode,
            title: result.StatusCode switch
            {
                StatusCodes.Status404NotFound => "Not Found",
                StatusCodes.Status403Forbidden => "Forbidden",
                StatusCodes.Status409Conflict => "Conflict",
                _ => "Bad Request"
            });
    }
}
