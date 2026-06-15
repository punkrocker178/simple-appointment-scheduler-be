using Infrastructure.Common;
using Infrastructure.ServiceTypes.Dtos;
using Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace universal_scheduler_be.Controllers;

[ApiController]
[Route("api/dealerships/{dealershipId:guid}/service-types")]
[Authorize]
public class ServiceTypeController : ControllerBase
{
    private readonly IServiceTypeService _serviceTypeService;

    public ServiceTypeController(IServiceTypeService serviceTypeService)
    {
        _serviceTypeService = serviceTypeService;
    }

    [HttpGet]
    [Authorize(Policy = "servicetypes:read")]
    [ProducesResponseType(typeof(IReadOnlyList<ServiceTypeResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetByDealership(
        Guid dealershipId,
        CancellationToken cancellationToken)
    {
        var result = await _serviceTypeService.GetByDealershipAsync(dealershipId, cancellationToken);
        return ToActionResult(result);
    }

    [HttpPost]
    [Authorize(Policy = "servicetypes:write")]
    [ProducesResponseType(typeof(ServiceTypeResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Create(
        Guid dealershipId,
        [FromBody] CreateServiceTypeRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _serviceTypeService.CreateAsync(dealershipId, request, cancellationToken);
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
    [Authorize(Policy = "servicetypes:write")]
    [ProducesResponseType(typeof(ServiceTypeResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Update(
        Guid dealershipId,
        Guid id,
        [FromBody] UpdateServiceTypeRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _serviceTypeService.UpdateAsync(dealershipId, id, request, cancellationToken);
        return ToActionResult(result);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "servicetypes:write")]
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
        var result = await _serviceTypeService.SoftDeleteAsync(dealershipId, id, cancellationToken);
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
