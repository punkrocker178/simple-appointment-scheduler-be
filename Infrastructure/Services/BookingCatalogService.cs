using Infrastructure.Booking.Dtos;
using Infrastructure.Common;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services;

public class BookingCatalogService : IBookingCatalogService
{
    private readonly ApplicationDbContext _db;

    public BookingCatalogService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<ServiceResult<BookingCatalogResponse>> GetCatalogAsync(
        CancellationToken cancellationToken = default)
    {
        var dealership = await _db.Dealerships
            .AsNoTracking()
            .OrderBy(d => d.Name)
            .FirstOrDefaultAsync(cancellationToken);

        if (dealership is null)
        {
            return ServiceResult<BookingCatalogResponse>.NotFound("No dealership available for booking.");
        }

        var serviceTypes = await _db.ServiceTypes
            .AsNoTracking()
            .Where(st => st.DealershipId == dealership.Id && st.IsActive)
            .OrderBy(st => st.Name)
            .Select(st => new BookingCatalogServiceType
            {
                Id = st.Id,
                Name = st.Name,
                Description = st.Description,
                DurationMinutes = st.DurationMinutes,
                Price = st.Price
            })
            .ToListAsync(cancellationToken);

        return ServiceResult<BookingCatalogResponse>.Ok(new BookingCatalogResponse
        {
            DealershipId = dealership.Id,
            DealershipName = dealership.Name,
            ServiceTypes = serviceTypes
        });
    }
}
