namespace Infrastructure.ServiceBays.Dtos;

public class ServiceBayResponse
{
    public Guid Id { get; set; }
    public Guid DealershipId { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}
