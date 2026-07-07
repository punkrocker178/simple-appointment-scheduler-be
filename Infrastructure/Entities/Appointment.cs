namespace Infrastructure.Entities;

public class Appointment
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public Guid VehicleId { get; set; }
    public Guid TechnicianId { get; set; }
    public Guid ServiceBayId { get; set; }
    public Guid ServiceTypeId { get; set; }
    public DateOnly BookingDate { get; set; }
    public int SecondsFromMidnight { get; set; }
    public AppointmentStatus Status { get; set; } = AppointmentStatus.Scheduled;
    public string? CancellationReason { get; set; }
    public DateTime? StartedAtUtc { get; set; }
    public DateTime? CompletedAtUtc { get; set; }
    public DateTime? CancelledAtUtc { get; set; }

    public Customer Customer { get; set; } = null!;
    public Vehicle Vehicle { get; set; } = null!;
    public Technician Technician { get; set; } = null!;
    public ServiceBay ServiceBay { get; set; } = null!;
    public ServiceType ServiceType { get; set; } = null!;
}

public enum AppointmentStatus
{
    Scheduled = 0,
    InProgress = 1,
    Completed = 2,
    Cancelled = 3
}
