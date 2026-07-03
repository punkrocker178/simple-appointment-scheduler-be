namespace Infrastructure.Appointments.Dtos;

public class CreateAppointmentRequest
{
    public Guid CustomerId { get; set; }
    public Guid VehicleId { get; set; }
    public Guid ServiceTypeId { get; set; }
    public DateOnly BookingDate { get; set; }
    public int SecondsFromMidnight { get; set; }
}
