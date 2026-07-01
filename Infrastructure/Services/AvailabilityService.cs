using Infrastructure.Appointments;
using Infrastructure.Appointments.Dtos;
using Infrastructure.Common;
using Infrastructure.Data;
using Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services;

public class AvailabilityService : IAvailabilityService
{
    private readonly ApplicationDbContext _db;
    private readonly TimeProvider _timeProvider;

    public AvailabilityService(ApplicationDbContext db, TimeProvider timeProvider)
    {
        _db = db;
        _timeProvider = timeProvider;
    }

    public async Task<ServiceResult<AvailabilityResponse>> GetAvailabilityAsync(
        Guid dealershipId,
        Guid serviceTypeId,
        DateOnly bookingDate,
        CancellationToken cancellationToken = default)
    {
        var serviceType = await _db.ServiceTypes
            .AsNoTracking()
            .FirstOrDefaultAsync(st => st.Id == serviceTypeId, cancellationToken);

        if (serviceType is null)
        {
            return ServiceResult<AvailabilityResponse>.NotFound("Service type not found.");
        }

        if (!serviceType.IsActive || serviceType.DealershipId != dealershipId)
        {
            return ServiceResult<AvailabilityResponse>.NotFound("Service type not found.");
        }

        var dealership = await _db.Dealerships
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.Id == dealershipId, cancellationToken);

        if (dealership is null)
        {
            return ServiceResult<AvailabilityResponse>.NotFound("Dealership not found.");
        }

        var technicians = await _db.Technicians
            .AsNoTracking()
            .Where(t => t.DealershipId == dealershipId && t.IsActive)
            .Select(t => new AvailabilityTechnician(
                t.Id,
                t.FirstName,
                t.LastName,
                t.TechnicianSkills.Select(ts => ts.SkillId).ToHashSet()))
            .ToListAsync(cancellationToken);

        var bays = await _db.ServiceBays
            .AsNoTracking()
            .Where(b => b.DealershipId == dealershipId && b.IsActive)
            .Select(b => new AvailabilityBay(b.Id, b.Name))
            .ToListAsync(cancellationToken);

        var technicianIds = technicians.Select(t => t.Id).ToList();
        var bayIds = bays.Select(b => b.Id).ToList();

        var appointments = await _db.Appointments
            .AsNoTracking()
            .Where(a => a.BookingDate == bookingDate
                && (technicianIds.Contains(a.TechnicianId) || bayIds.Contains(a.ServiceBayId)))
            .Select(a => new ExistingAppointmentRecord(
                a.TechnicianId,
                a.ServiceBayId,
                a.SecondsFromMidnight,
                a.ServiceType.DurationMinutes * 60,
                a.Status))
            .ToListAsync(cancellationToken);

        var slots = AvailabilityEngine.GetAvailableSlots(
            dealership.OpenSecondsFromMidnight,
            dealership.CloseSecondsFromMidnight,
            serviceType.DurationMinutes,
            serviceType.SkillId,
            technicians,
            bays,
            appointments,
            bookingDate,
            _timeProvider.GetUtcNow().UtcDateTime);

        return ServiceResult<AvailabilityResponse>.Ok(new AvailabilityResponse
        {
            BookingDate = bookingDate,
            ServiceTypeId = serviceTypeId,
            DurationMinutes = serviceType.DurationMinutes,
            Slots = slots.Select(s => new AvailabilitySlotDto { SecondsFromMidnight = s }).ToList()
        });
    }
}
