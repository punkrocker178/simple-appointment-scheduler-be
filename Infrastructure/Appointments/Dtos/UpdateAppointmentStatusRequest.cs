using Infrastructure.Entities;

namespace Infrastructure.Appointments.Dtos;

public class UpdateAppointmentStatusRequest
{
    public AppointmentStatus Status { get; set; }
}
