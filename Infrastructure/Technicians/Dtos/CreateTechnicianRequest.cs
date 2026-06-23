namespace Infrastructure.Technicians.Dtos;

public class CreateTechnicianRequest
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public IReadOnlyList<Guid>? SkillIds { get; set; }
}
