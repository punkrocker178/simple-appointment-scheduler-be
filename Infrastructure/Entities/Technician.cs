namespace Infrastructure.Entities;

public class Technician
{
    public Guid Id { get; set; }
    public Guid DealershipId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;

    public Dealership Dealership { get; set; } = null!;
    public ICollection<TechnicianSkill> TechnicianSkills { get; } = [];
    public ICollection<Appointment> Appointments { get; } = [];
}