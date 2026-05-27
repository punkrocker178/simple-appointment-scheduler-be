namespace Infrastructure.Entities;
public class TechnicianSkill
{
    public Guid TechnicianId { get; set; }
    public Guid SkillId { get; set; }

    public Technician Technician { get; set; } = null!;
    public Skill Skill { get; set; } = null!;
}