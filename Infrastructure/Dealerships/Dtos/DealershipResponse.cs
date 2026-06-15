namespace Infrastructure.Dealerships.Dtos;

public class DealershipResponse
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Address { get; init; } = string.Empty;
    public string Phone { get; init; } = string.Empty;
    public string Timezone { get; init; } = string.Empty;
}
