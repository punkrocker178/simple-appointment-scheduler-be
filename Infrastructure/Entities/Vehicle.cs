namespace Infrastructure.Entities;
public class Vehicle
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public string Make { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public int Year { get; set; }

    public Customer Customer { get; set; } = null!;
    public ICollection<Appointment> Appointments { get; } = [];
}