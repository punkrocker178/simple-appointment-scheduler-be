using Infrastructure.Booking.Dtos;
using Infrastructure.Common;

namespace Infrastructure.Services;

public interface IBookingCatalogService
{
    Task<ServiceResult<BookingCatalogResponse>> GetCatalogAsync(
        CancellationToken cancellationToken = default);
}
