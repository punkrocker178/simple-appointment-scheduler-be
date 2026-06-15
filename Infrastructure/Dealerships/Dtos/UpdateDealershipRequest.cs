namespace Infrastructure.Dealerships.Dtos;

public class UpdateDealershipRequest
{
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Timezone { get; set; } = string.Empty;
}
