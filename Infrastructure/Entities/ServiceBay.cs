namespace Infrastructure.Entities;
public class ServiceBay
{
    public Guid Id { get; set; }
    public Guid DealershipId { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;

    public Dealership Dealership { get; set; } = null!;
    public ICollection<Appointment> Appointments { get; } = [];
}