using Infrastructure.Common;
using Infrastructure.Services;
using Infrastructure.Technicians.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace universal_scheduler_be.Controllers;

[ApiController]
[Route("api/dealerships/{dealershipId:guid}/technicians")]
[Authorize]
public class TechnicianController : ControllerBase
{
    private readonly ITechnicianService _technicianService;

    public TechnicianController(ITechnicianService technicianService)
    {
        _technicianService = technicianService;
    }

    [HttpGet]
    [Authorize(Policy = "technicians:read")]
    [ProducesResponseType(typeof(IReadOnlyList<TechnicianResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetByDealership(
        Guid dealershipId,
        CancellationToken cancellationToken)
    {
        var result = await _technicianService.GetByDealershipAsync(dealershipId, cancellationToken);
        return ToActionResult(result);
    }

    [HttpPost]
    [Authorize(Policy = "technicians:write")]
    [ProducesResponseType(typeof(TechnicianResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Create(
        Guid dealershipId,
        [FromBody] CreateTechnicianRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _technicianService.CreateAsync(dealershipId, request, cancellationToken);
        if (result.Data is not null && result.StatusCode == StatusCodes.Status201Created)
        {
            return CreatedAtAction(
                nameof(GetByDealership),
                new { dealershipId },
                result.Data);
        }

        return ToActionResult(result);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = "technicians:write")]
    [ProducesResponseType(typeof(TechnicianResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Update(
        Guid dealershipId,
        Guid id,
        [FromBody] UpdateTechnicianRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _technicianService.UpdateAsync(dealershipId, id, request, cancellationToken);
        return ToActionResult(result);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "technicians:write")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> SoftDelete(
        Guid dealershipId,
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await _technicianService.SoftDeleteAsync(dealershipId, id, cancellationToken);
        return ToActionResult(result);
    }

    private IActionResult ToActionResult<T>(ServiceResult<T> result)
    {
        if (result.StatusCode == StatusCodes.Status204NoContent)
        {
            return NoContent();
        }

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
                StatusCodes.Status409Conflict => "Conflict",
                _ => "Bad Request"
            });
    }
}
