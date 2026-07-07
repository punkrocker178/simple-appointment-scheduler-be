namespace Infrastructure.Booking.Dtos;

public class BookingCatalogResponse
{
    public Guid DealershipId { get; set; }
    public string DealershipName { get; set; } = string.Empty;
    public IReadOnlyList<BookingCatalogServiceType> ServiceTypes { get; set; } = [];
}

public class BookingCatalogServiceType
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int DurationMinutes { get; set; }
    public decimal Price { get; set; }
}
