using Infrastructure.Entities;

namespace Infrastructure.Appointments;

public static class AppointmentSchedulingConstants
{
    public const int SlotStepSeconds = 1800;
    public const int DefaultOpenSecondsFromMidnight = 28_800;
    public const int DefaultCloseSecondsFromMidnight = 61_200;
    public const int SecondsPerDay = 86_400;
}

public record AvailabilityTechnician(Guid Id, string FirstName, string LastName, IReadOnlySet<Guid> SkillIds);

public record AvailabilityBay(Guid Id, string Name);

public record ExistingAppointmentRecord(
    Guid TechnicianId,
    Guid ServiceBayId,
    int SecondsFromMidnight,
    int DurationSeconds,
    AppointmentStatus Status);

public record SlotAssignment(Guid TechnicianId, Guid ServiceBayId);

public record AvailabilitySlot(int SecondsFromMidnight, bool Available);

public static class AvailabilityEngine
{
    public static IReadOnlyList<AvailabilitySlot> GetSlots(
        int openSeconds,
        int closeSeconds,
        int durationMinutes,
        Guid requiredSkillId,
        IReadOnlyList<AvailabilityTechnician> technicians,
        IReadOnlyList<AvailabilityBay> bays,
        IReadOnlyList<ExistingAppointmentRecord> appointments,
        DateOnly bookingDate,
        DateTime utcNow)
    {
        var durationSeconds = durationMinutes * 60;
        var blockingAppointments = appointments
            .Where(a => IsBlockingStatus(a.Status))
            .ToList();

        var qualifiedTechnicians = technicians
            .Where(t => t.SkillIds.Contains(requiredSkillId))
            .OrderBy(t => t.LastName)
            .ThenBy(t => t.FirstName)
            .ThenBy(t => t.Id)
            .ToList();

        var orderedBays = bays
            .OrderBy(b => b.Name)
            .ThenBy(b => b.Id)
            .ToList();

        var todayUtc = DateOnly.FromDateTime(utcNow);
        var currentSeconds = utcNow.Hour * 3600 + utcNow.Minute * 60 + utcNow.Second;
        var hasResources = qualifiedTechnicians.Count > 0 && orderedBays.Count > 0;
        var slots = new List<AvailabilitySlot>();

        for (var start = openSeconds; start <= closeSeconds - durationSeconds; start += AppointmentSchedulingConstants.SlotStepSeconds)
        {
            if (start + durationSeconds > closeSeconds)
            {
                continue;
            }

            var isPast = bookingDate == todayUtc && start < currentSeconds;
            var isAvailable = !isPast && hasResources
                && TryAssignSlot(start, durationSeconds, qualifiedTechnicians, orderedBays, blockingAppointments) is not null;

            slots.Add(new AvailabilitySlot(start, isAvailable));
        }

        return slots;
    }

    public static SlotAssignment? TryAssignSlot(
        int secondsFromMidnight,
        int durationSeconds,
        IReadOnlyList<AvailabilityTechnician> qualifiedTechnicians,
        IReadOnlyList<AvailabilityBay> bays,
        IReadOnlyList<ExistingAppointmentRecord> appointments)
    {
        var blockingAppointments = appointments
            .Where(a => IsBlockingStatus(a.Status))
            .ToList();

        foreach (var technician in qualifiedTechnicians)
        {
            if (HasOverlap(technician.Id, null, secondsFromMidnight, durationSeconds, blockingAppointments))
            {
                continue;
            }

            foreach (var bay in bays)
            {
                if (HasOverlap(null, bay.Id, secondsFromMidnight, durationSeconds, blockingAppointments))
                {
                    continue;
                }

                return new SlotAssignment(technician.Id, bay.Id);
            }
        }

        return null;
    }

    public static bool IntervalsOverlap(int startA, int endA, int startB, int endB) =>
        startA < endB && endA > startB;

    public static bool IsBlockingStatus(AppointmentStatus status) =>
        status is AppointmentStatus.Scheduled or AppointmentStatus.InProgress;

    public static bool IsOnSlotGrid(int secondsFromMidnight) =>
        secondsFromMidnight % AppointmentSchedulingConstants.SlotStepSeconds == 0;

    public static bool IsWithinBusinessHours(
        int secondsFromMidnight,
        int durationMinutes,
        int openSeconds,
        int closeSeconds)
    {
        var durationSeconds = durationMinutes * 60;
        return secondsFromMidnight >= openSeconds
            && secondsFromMidnight + durationSeconds <= closeSeconds;
    }

    public static string? ValidateBusinessHours(int openSeconds, int closeSeconds)
    {
        if (openSeconds < 0 || closeSeconds > AppointmentSchedulingConstants.SecondsPerDay)
        {
            return "Business hours must be between 0 and 86400 seconds.";
        }

        if (openSeconds >= closeSeconds)
        {
            return "Open time must be before close time.";
        }

        if (openSeconds % AppointmentSchedulingConstants.SlotStepSeconds != 0
            || closeSeconds % AppointmentSchedulingConstants.SlotStepSeconds != 0)
        {
            return "Business hours must align to 30-minute increments.";
        }

        return null;
    }

    private static bool HasOverlap(
        Guid? technicianId,
        Guid? bayId,
        int startSeconds,
        int durationSeconds,
        IReadOnlyList<ExistingAppointmentRecord> appointments)
    {
        var endSeconds = startSeconds + durationSeconds;

        foreach (var appointment in appointments)
        {
            if (technicianId.HasValue && appointment.TechnicianId != technicianId.Value)
            {
                continue;
            }

            if (bayId.HasValue && appointment.ServiceBayId != bayId.Value)
            {
                continue;
            }

            var appointmentEnd = appointment.SecondsFromMidnight + appointment.DurationSeconds;
            if (IntervalsOverlap(startSeconds, endSeconds, appointment.SecondsFromMidnight, appointmentEnd))
            {
                return true;
            }
        }

        return false;
    }
}
