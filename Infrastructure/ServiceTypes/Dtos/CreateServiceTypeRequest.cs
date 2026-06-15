namespace Infrastructure.ServiceTypes.Dtos;

public class CreateServiceTypeRequest
{
    public Guid SkillId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int DurationMinutes { get; set; }
    public decimal Price { get; set; }
}
