using Infrastructure.Common;
using Infrastructure.ServiceBays.Dtos;
using Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace universal_scheduler_be.Controllers;

[ApiController]
[Route("api/dealerships/{dealershipId:guid}/service-bays")]
[Authorize]
public class ServiceBayController : ControllerBase
{
    private readonly IServiceBayService _serviceBayService;

    public ServiceBayController(IServiceBayService serviceBayService)
    {
        _serviceBayService = serviceBayService;
    }

    [HttpGet]
    [Authorize(Policy = "servicebays:read")]
    [ProducesResponseType(typeof(IReadOnlyList<ServiceBayResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetByDealership(
        Guid dealershipId,
        CancellationToken cancellationToken)
    {
        var result = await _serviceBayService.GetByDealershipAsync(dealershipId, cancellationToken);
        return ToActionResult(result);
    }

    [HttpPost]
    [Authorize(Policy = "servicebays:write")]
    [ProducesResponseType(typeof(ServiceBayResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Create(
        Guid dealershipId,
        [FromBody] CreateServiceBayRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _serviceBayService.CreateAsync(dealershipId, request, cancellationToken);
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
    [Authorize(Policy = "servicebays:write")]
    [ProducesResponseType(typeof(ServiceBayResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Update(
        Guid dealershipId,
        Guid id,
        [FromBody] UpdateServiceBayRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _serviceBayService.UpdateAsync(dealershipId, id, request, cancellationToken);
        return ToActionResult(result);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "servicebays:write")]
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
        var result = await _serviceBayService.SoftDeleteAsync(dealershipId, id, cancellationToken);
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
