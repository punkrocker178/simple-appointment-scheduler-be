using Infrastructure.Appointments;
using Infrastructure.Appointments.Dtos;
using Infrastructure.Common;
using Infrastructure.Data;
using Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services;

public class AppointmentService : IAppointmentService
{
    private readonly ApplicationDbContext _db;
    private readonly TimeProvider _timeProvider;

    public AppointmentService(ApplicationDbContext db, TimeProvider timeProvider)
    {
        _db = db;
        _timeProvider = timeProvider;
    }

    public async Task<ServiceResult<AppointmentResponse>> CreateAsync(
        CreateAppointmentRequest request,
        CancellationToken cancellationToken = default)
    {
        var customer = await _db.Customers
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == request.CustomerId, cancellationToken);

        if (customer is null)
        {
            return ServiceResult<AppointmentResponse>.NotFound("Customer not found.");
        }

        var vehicle = await _db.Vehicles
            .AsNoTracking()
            .FirstOrDefaultAsync(v => v.Id == request.VehicleId, cancellationToken);

        if (vehicle is null)
        {
            return ServiceResult<AppointmentResponse>.NotFound("Vehicle not found.");
        }

        if (vehicle.CustomerId != request.CustomerId)
        {
            return ServiceResult<AppointmentResponse>.BadRequest("Vehicle does not belong to the customer.");
        }

        var serviceType = await _db.ServiceTypes
            .AsNoTracking()
            .FirstOrDefaultAsync(st => st.Id == request.ServiceTypeId, cancellationToken);

        if (serviceType is null || !serviceType.IsActive)
        {
            return ServiceResult<AppointmentResponse>.NotFound("Service type not found.");
        }

        var dealership = await _db.Dealerships
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.Id == serviceType.DealershipId, cancellationToken);

        if (dealership is null)
        {
            return ServiceResult<AppointmentResponse>.NotFound("Dealership not found.");
        }

        if (!AvailabilityEngine.IsOnSlotGrid(request.SecondsFromMidnight))
        {
            return ServiceResult<AppointmentResponse>.BadRequest("Start time must align to 30-minute increments.");
        }

        if (!AvailabilityEngine.IsWithinBusinessHours(
                request.SecondsFromMidnight,
                serviceType.DurationMinutes,
                dealership.OpenSecondsFromMidnight,
                dealership.CloseSecondsFromMidnight))
        {
            return ServiceResult<AppointmentResponse>.BadRequest("Requested time is outside business hours.");
        }

        var technicians = await _db.Technicians
            .AsNoTracking()
            .Where(t => t.DealershipId == serviceType.DealershipId && t.IsActive)
            .Select(t => new AvailabilityTechnician(
                t.Id,
                t.FirstName,
                t.LastName,
                t.TechnicianSkills.Select(ts => ts.SkillId).ToHashSet()))
            .ToListAsync(cancellationToken);

        var qualifiedTechnicians = technicians
            .Where(t => t.SkillIds.Contains(serviceType.SkillId))
            .OrderBy(t => t.LastName)
            .ThenBy(t => t.FirstName)
            .ThenBy(t => t.Id)
            .ToList();

        var bays = await _db.ServiceBays
            .AsNoTracking()
            .Where(b => b.DealershipId == serviceType.DealershipId && b.IsActive)
            .OrderBy(b => b.Name)
            .ThenBy(b => b.Id)
            .Select(b => new AvailabilityBay(b.Id, b.Name))
            .ToListAsync(cancellationToken);

        if (qualifiedTechnicians.Count == 0 || bays.Count == 0)
        {
            return ServiceResult<AppointmentResponse>.BadRequest("No resources available for this service type.");
        }

        var technicianIds = technicians.Select(t => t.Id).ToList();
        var bayIds = bays.Select(b => b.Id).ToList();
        var durationSeconds = serviceType.DurationMinutes * 60;

        var existingAppointments = await _db.Appointments
            .AsNoTracking()
            .Where(a => a.BookingDate == request.BookingDate
                && (technicianIds.Contains(a.TechnicianId) || bayIds.Contains(a.ServiceBayId)))
            .Select(a => new ExistingAppointmentRecord(
                a.TechnicianId,
                a.ServiceBayId,
                a.SecondsFromMidnight,
                a.ServiceType.DurationMinutes * 60,
                a.Status))
            .ToListAsync(cancellationToken);

        var todayUtc = DateOnly.FromDateTime(_timeProvider.GetUtcNow().UtcDateTime);
        if (request.BookingDate == todayUtc)
        {
            var currentSeconds = _timeProvider.GetUtcNow().UtcDateTime;
            var secondsNow = currentSeconds.Hour * 3600 + currentSeconds.Minute * 60 + currentSeconds.Second;
            if (request.SecondsFromMidnight < secondsNow)
            {
                return ServiceResult<AppointmentResponse>.BadRequest("Cannot book a slot in the past.");
            }
        }

        var assignment = AvailabilityEngine.TryAssignSlot(
            request.SecondsFromMidnight,
            durationSeconds,
            qualifiedTechnicians,
            bays,
            existingAppointments);

        if (assignment is null)
        {
            return ServiceResult<AppointmentResponse>.Conflict("The requested time slot is no longer available.");
        }

        var appointment = new Appointment
        {
            Id = Guid.NewGuid(),
            CustomerId = request.CustomerId,
            VehicleId = request.VehicleId,
            ServiceTypeId = request.ServiceTypeId,
            TechnicianId = assignment.TechnicianId,
            ServiceBayId = assignment.ServiceBayId,
            BookingDate = request.BookingDate,
            SecondsFromMidnight = request.SecondsFromMidnight,
            Status = AppointmentStatus.Scheduled
        };

        _db.Appointments.Add(appointment);

        try
        {
            await _db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            return ServiceResult<AppointmentResponse>.Conflict("The requested time slot is no longer available.");
        }

        return ServiceResult<AppointmentResponse>.Created(ToResponse(appointment, serviceType.DurationMinutes));
    }

    public async Task<ServiceResult<AppointmentResponse>> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var appointment = await _db.Appointments
            .AsNoTracking()
            .Include(a => a.ServiceType)
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);

        if (appointment is null)
        {
            return ServiceResult<AppointmentResponse>.NotFound("Appointment not found.");
        }

        return ServiceResult<AppointmentResponse>.Ok(ToResponse(appointment, appointment.ServiceType.DurationMinutes));
    }

    public async Task<ServiceResult<IReadOnlyList<AppointmentResponse>>> GetByCustomerAsync(
        Guid customerId,
        CancellationToken cancellationToken = default)
    {
        var customerExists = await _db.Customers
            .AsNoTracking()
            .AnyAsync(c => c.Id == customerId, cancellationToken);

        if (!customerExists)
        {
            return ServiceResult<IReadOnlyList<AppointmentResponse>>.NotFound("Customer not found.");
        }

        var appointments = await _db.Appointments
            .AsNoTracking()
            .Include(a => a.ServiceType)
            .Where(a => a.CustomerId == customerId)
            .OrderBy(a => a.BookingDate)
            .ThenBy(a => a.SecondsFromMidnight)
            .ToListAsync(cancellationToken);

        return ServiceResult<IReadOnlyList<AppointmentResponse>>.Ok(
            appointments.Select(a => ToResponse(a, a.ServiceType.DurationMinutes)).ToList());
    }

    public async Task<ServiceResult<IReadOnlyList<AppointmentResponse>>> GetByDealershipAndDateAsync(
        Guid dealershipId,
        DateOnly date,
        CancellationToken cancellationToken = default)
    {
        var dealershipExists = await _db.Dealerships
            .AsNoTracking()
            .AnyAsync(d => d.Id == dealershipId, cancellationToken);

        if (!dealershipExists)
        {
            return ServiceResult<IReadOnlyList<AppointmentResponse>>.NotFound("Dealership not found.");
        }

        var appointments = await _db.Appointments
            .AsNoTracking()
            .Include(a => a.ServiceType)
            .Where(a => a.BookingDate == date && a.ServiceType.DealershipId == dealershipId)
            .OrderBy(a => a.SecondsFromMidnight)
            .ToListAsync(cancellationToken);

        return ServiceResult<IReadOnlyList<AppointmentResponse>>.Ok(
            appointments.Select(a => ToResponse(a, a.ServiceType.DurationMinutes)).ToList());
    }

    private static AppointmentResponse ToResponse(Appointment appointment, int durationMinutes) =>
        new()
        {
            Id = appointment.Id,
            CustomerId = appointment.CustomerId,
            VehicleId = appointment.VehicleId,
            ServiceTypeId = appointment.ServiceTypeId,
            TechnicianId = appointment.TechnicianId,
            ServiceBayId = appointment.ServiceBayId,
            BookingDate = appointment.BookingDate,
            SecondsFromMidnight = appointment.SecondsFromMidnight,
            DurationMinutes = durationMinutes,
            Status = appointment.Status
        };
}
