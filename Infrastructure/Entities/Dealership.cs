namespace Infrastructure.Entities;
public class Dealership
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Timezone { get; set; } = string.Empty;
    public int OpenSecondsFromMidnight { get; set; } = 28_800;
    public int CloseSecondsFromMidnight { get; set; } = 61_200;

    public ICollection<Technician> Technicians { get; } = [];
    public ICollection<ServiceBay> ServiceBays { get; } = [];
    public ICollection<ServiceType> ServiceTypes { get; } = [];
}