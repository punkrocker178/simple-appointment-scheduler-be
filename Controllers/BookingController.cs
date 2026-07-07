using Infrastructure.Booking.Dtos;
using Infrastructure.Common;
using Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace universal_scheduler_be.Controllers;

[ApiController]
[Route("api/booking")]
[Authorize]
public class BookingController : ControllerBase
{
    private readonly IBookingCatalogService _bookingCatalogService;

    public BookingController(IBookingCatalogService bookingCatalogService)
    {
        _bookingCatalogService = bookingCatalogService;
    }

    [HttpGet("catalog")]
    [ProducesResponseType(typeof(BookingCatalogResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetCatalog(CancellationToken cancellationToken)
    {
        var result = await _bookingCatalogService.GetCatalogAsync(cancellationToken);
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
                _ => "Bad Request"
            });
    }
}
