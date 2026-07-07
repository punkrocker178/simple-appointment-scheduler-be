using Infrastructure.Appointments;
using Infrastructure.Entities;

namespace universal_scheduler_be.Tests.Appointments;

public class AppointmentStatusTransitionsTests
{
    [Theory]
    [InlineData(AppointmentStatus.Scheduled, AppointmentStatus.InProgress, true)]
    [InlineData(AppointmentStatus.Scheduled, AppointmentStatus.Cancelled, true)]
    [InlineData(AppointmentStatus.InProgress, AppointmentStatus.Completed, true)]
    [InlineData(AppointmentStatus.InProgress, AppointmentStatus.Cancelled, true)]
    [InlineData(AppointmentStatus.Scheduled, AppointmentStatus.Completed, false)]
    [InlineData(AppointmentStatus.Scheduled, AppointmentStatus.Scheduled, false)]
    [InlineData(AppointmentStatus.Completed, AppointmentStatus.Scheduled, false)]
    [InlineData(AppointmentStatus.Cancelled, AppointmentStatus.InProgress, false)]
    public void CanTransition_ReturnsExpected(
        AppointmentStatus from,
        AppointmentStatus to,
        bool expected)
    {
        Assert.Equal(expected, AppointmentStatusTransitions.CanTransition(from, to));
    }

    [Fact]
    public void GetTransitionError_ScheduledToCompleted_ReturnsMustStartFirstMessage()
    {
        var error = AppointmentStatusTransitions.GetTransitionError(
            AppointmentStatus.Scheduled,
            AppointmentStatus.Completed);

        Assert.Equal("Appointment must be started before it can be completed.", error);
    }

    [Fact]
    public void GetTransitionError_TerminalState_ReturnsTerminalMessage()
    {
        var error = AppointmentStatusTransitions.GetTransitionError(
            AppointmentStatus.Completed,
            AppointmentStatus.Cancelled);

        Assert.Equal("Appointment is in a terminal state and cannot be updated.", error);
    }

    [Theory]
    [InlineData(AppointmentStatus.Completed, true)]
    [InlineData(AppointmentStatus.Cancelled, true)]
    [InlineData(AppointmentStatus.Scheduled, false)]
    [InlineData(AppointmentStatus.InProgress, false)]
    public void IsTerminal_ReturnsExpected(AppointmentStatus status, bool expected)
    {
        Assert.Equal(expected, AppointmentStatusTransitions.IsTerminal(status));
    }
}
