using Infrastructure.Common;
using Infrastructure.Services;
using Infrastructure.Skills.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace universal_scheduler_be.Controllers;

[ApiController]
[Route("api/skills")]
[Authorize]
public class SkillController : ControllerBase
{
    private readonly ISkillService _skillService;

    public SkillController(ISkillService skillService)
    {
        _skillService = skillService;
    }

    [HttpGet]
    [Authorize(Policy = "skills:read")]
    [ProducesResponseType(typeof(IReadOnlyList<SkillResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var result = await _skillService.GetAllAsync(cancellationToken);
        return ToActionResult(result);
    }

    [HttpPost]
    [Authorize(Policy = "skills:write")]
    [ProducesResponseType(typeof(SkillResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Create(
        [FromBody] CreateSkillRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _skillService.CreateAsync(request, cancellationToken);
        if (result.Data is not null && result.StatusCode == StatusCodes.Status201Created)
        {
            return CreatedAtAction(nameof(GetAll), result.Data);
        }

        return ToActionResult(result);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "skills:write")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var result = await _skillService.DeleteAsync(id, cancellationToken);
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
