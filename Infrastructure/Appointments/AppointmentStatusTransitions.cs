using Infrastructure.Entities;

namespace Infrastructure.Appointments;

public static class AppointmentStatusTransitions
{
    private static readonly IReadOnlyDictionary<AppointmentStatus, HashSet<AppointmentStatus>> Allowed =
        new Dictionary<AppointmentStatus, HashSet<AppointmentStatus>>
        {
            [AppointmentStatus.Scheduled] =
            [
                AppointmentStatus.InProgress,
                AppointmentStatus.Cancelled
            ],
            [AppointmentStatus.InProgress] =
            [
                AppointmentStatus.Completed,
                AppointmentStatus.Cancelled
            ]
        };

    public static bool CanTransition(AppointmentStatus from, AppointmentStatus to) =>
        Allowed.TryGetValue(from, out var targets) && targets.Contains(to);

    public static string? GetTransitionError(AppointmentStatus from, AppointmentStatus to)
    {
        if (from is AppointmentStatus.Completed or AppointmentStatus.Cancelled)
        {
            return "Appointment is in a terminal state and cannot be updated.";
        }

        if (from == AppointmentStatus.Scheduled && to == AppointmentStatus.Completed)
        {
            return "Appointment must be started before it can be completed.";
        }

        if (!CanTransition(from, to))
        {
            return $"Cannot transition appointment from {from} to {to}.";
        }

        return null;
    }

    public static bool IsTerminal(AppointmentStatus status) =>
        status is AppointmentStatus.Completed or AppointmentStatus.Cancelled;
}
