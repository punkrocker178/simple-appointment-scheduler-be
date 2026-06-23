namespace Infrastructure.Technicians.Dtos;

public class UpdateTechnicianRequest
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public IReadOnlyList<Guid>? SkillIds { get; set; }
}
