using Infrastructure.Booking.Dtos;
using Infrastructure.Common;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services;

public class DefaultDealershipService : IDefaultDealershipService
{
    private readonly ApplicationDbContext _db;

    public DefaultDealershipService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<ServiceResult<DefaultDealershipResponse>> GetDefaultDealershipAsync(
        CancellationToken cancellationToken = default)
    {
        var dealership = await _db.Dealerships
            .AsNoTracking()
            .OrderBy(d => d.Name)
            .FirstOrDefaultAsync(cancellationToken);

        if (dealership is null)
        {
            return ServiceResult<DefaultDealershipResponse>.NotFound("No dealership available for booking.");
        }

        return ServiceResult<DefaultDealershipResponse>.Ok(new DefaultDealershipResponse
        {
            DealershipId = dealership.Id,
            DealershipName = dealership.Name
        });
    }
}
