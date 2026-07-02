namespace Infrastructure.Dealerships.Dtos;

public class CreateDealershipRequest
{
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Timezone { get; set; } = string.Empty;
    public int OpenSecondsFromMidnight { get; set; } = 28_800;
    public int CloseSecondsFromMidnight { get; set; } = 61_200;
}
