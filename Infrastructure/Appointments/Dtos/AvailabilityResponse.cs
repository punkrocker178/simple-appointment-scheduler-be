namespace Infrastructure.Appointments.Dtos;

public class AvailabilitySlotDto
{
    public int SecondsFromMidnight { get; init; }
    public bool Available { get; init; }
}

public class AvailabilityResponse
{
    public DateOnly BookingDate { get; init; }
    public Guid ServiceTypeId { get; init; }
    public int DurationMinutes { get; init; }
    public IReadOnlyList<AvailabilitySlotDto> Slots { get; init; } = [];
}
