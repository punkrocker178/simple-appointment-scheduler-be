namespace Infrastructure.Entities;
public class Skill
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    public ICollection<TechnicianSkill> TechnicianSkills { get; } = [];
    public ICollection<ServiceType> ServiceTypes { get; } = [];
}