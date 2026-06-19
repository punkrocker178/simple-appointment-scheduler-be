namespace Infrastructure.Technicians.Dtos;

public class TechnicianResponse
{
    public Guid Id { get; set; }
    public Guid DealershipId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}
