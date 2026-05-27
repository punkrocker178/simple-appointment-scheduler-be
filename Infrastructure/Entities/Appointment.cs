namespace Infrastructure.Entities;
public class Appointment
{
    public Guid Id { get; private set; }
    public Guid CustomerId { get; private set; }
    public Guid TechnicianId { get; private set; }
    public Guid ServiceBayId { get; private set; }
    public Guid ServiceTypeId { get; private set; }
    public DateTime StartTime { get; private set; }
    public DateTime EndTime { get; private set; }
    public enum Status
    {
        Scheduled,
        InProgress,
        Completed,
        Cancelled
    }

    public Customer Customer { get; set; } = null!;
    public Vehicle Vehicle { get; set; } = null!;
    public Technician Technician { get; set; } = null!;
    public ServiceBay ServiceBay { get; set; } = null!;
    public ServiceType ServiceType { get; set; } = null!;

}