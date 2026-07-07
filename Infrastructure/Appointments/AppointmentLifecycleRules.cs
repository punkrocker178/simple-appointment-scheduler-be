using Infrastructure.Entities;

namespace Infrastructure.Appointments;

public static class AppointmentLifecycleRules
{
    public static readonly TimeSpan CancellationCutoff = TimeSpan.FromHours(2);

    public static DateTime GetAppointmentStartUtc(DateOnly bookingDate, int secondsFromMidnight) =>
        bookingDate.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc).AddSeconds(secondsFromMidnight);

    public static bool IsWithinCancellationCutoff(
        DateOnly bookingDate,
        int secondsFromMidnight,
        DateTimeOffset utcNow) =>
        GetAppointmentStartUtc(bookingDate, secondsFromMidnight) - utcNow.UtcDateTime < CancellationCutoff;

    public static bool IsStaffOrAdmin(string role) =>
        role is "Admin" or "Staff";

    public static bool CanCustomerCancel(AppointmentStatus status) =>
        status == AppointmentStatus.Scheduled;

    public static bool CanStaffCancel(AppointmentStatus status) =>
        status is AppointmentStatus.Scheduled or AppointmentStatus.InProgress;
}
