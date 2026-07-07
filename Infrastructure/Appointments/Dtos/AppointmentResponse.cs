using Infrastructure.Entities;

namespace Infrastructure.Appointments.Dtos;

public class AppointmentResponse
{
    public Guid Id { get; init; }
    public Guid CustomerId { get; init; }
    public Guid VehicleId { get; init; }
    public Guid ServiceTypeId { get; init; }
    public Guid TechnicianId { get; init; }
    public Guid ServiceBayId { get; init; }
    public DateOnly BookingDate { get; init; }
    public int SecondsFromMidnight { get; init; }
    public int DurationMinutes { get; init; }
    public AppointmentStatus Status { get; init; }
    public string? CancellationReason { get; init; }
    public DateTime? StartedAtUtc { get; init; }
    public DateTime? CompletedAtUtc { get; init; }
    public DateTime? CancelledAtUtc { get; init; }
}
