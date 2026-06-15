namespace Infrastructure.ServiceTypes.Dtos;

public class ServiceTypeResponse
{
    public Guid Id { get; set; }
    public Guid DealershipId { get; set; }
    public Guid SkillId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int DurationMinutes { get; set; }
    public decimal Price { get; set; }
    public bool IsActive { get; set; }
}
