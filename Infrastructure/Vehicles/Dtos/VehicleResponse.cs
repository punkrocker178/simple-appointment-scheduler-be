namespace Infrastructure.Vehicles.Dtos;

public class VehicleResponse
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public string Make { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public int Year { get; set; }
    public bool CanDelete { get; set; }
}
