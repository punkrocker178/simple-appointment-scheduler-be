using Infrastructure.Appointments;

namespace universal_scheduler_be.Tests.Appointments;

public class AppointmentLifecycleRulesTests
{
    private static readonly DateOnly BookingDate = new(2026, 7, 7);

    [Fact]
    public void IsWithinCancellationCutoff_ExactlyTwoHoursBeforeStart_ReturnsFalse()
    {
        var startUtc = AppointmentLifecycleRules.GetAppointmentStartUtc(BookingDate, 14 * 3600);
        var now = new DateTimeOffset(startUtc.AddHours(-2), TimeSpan.Zero);

        Assert.False(AppointmentLifecycleRules.IsWithinCancellationCutoff(BookingDate, 14 * 3600, now));
    }

    [Fact]
    public void IsWithinCancellationCutoff_OneHourFiftyNineMinutesBeforeStart_ReturnsTrue()
    {
        var startUtc = AppointmentLifecycleRules.GetAppointmentStartUtc(BookingDate, 14 * 3600);
        var now = new DateTimeOffset(startUtc.AddHours(-1).AddMinutes(-59), TimeSpan.Zero);

        Assert.True(AppointmentLifecycleRules.IsWithinCancellationCutoff(BookingDate, 14 * 3600, now));
    }

    [Fact]
    public void IsWithinCancellationCutoff_AfterStart_ReturnsTrue()
    {
        var startUtc = AppointmentLifecycleRules.GetAppointmentStartUtc(BookingDate, 14 * 3600);
        var now = new DateTimeOffset(startUtc.AddMinutes(30), TimeSpan.Zero);

        Assert.True(AppointmentLifecycleRules.IsWithinCancellationCutoff(BookingDate, 14 * 3600, now));
    }

    [Theory]
    [InlineData("Admin", true)]
    [InlineData("Staff", true)]
    [InlineData("User", false)]
    public void IsStaffOrAdmin_ReturnsExpected(string role, bool expected)
    {
        Assert.Equal(expected, AppointmentLifecycleRules.IsStaffOrAdmin(role));
    }
}
