# Phase 5 — Appointment Lifecycle Plan

**Status:** Complete (2026-07-07)

## Endpoints

| Method | Route | Who | Purpose |
|--------|-------|-----|---------|
| `PATCH` | `/api/appointments/{id}/status` | Staff, Admin | `Scheduled` → `InProgress`, `InProgress` → `Completed` |
| `POST` | `/api/appointments/{id}/cancel` | Customer (own), Staff, Admin | Cancel with required reason |

## Transition rules

- `Scheduled` → `InProgress` | `Cancelled`
- `InProgress` → `Completed` | `Cancelled`
- Direct `Scheduled` → `Completed` rejected (`409`)
- `Completed` / `Cancelled` are terminal

## Cancellation cutoff

- Customers cannot cancel within **2 hours** of appointment start (`400`)
- Staff/Admin may cancel at any time before terminal state
- Customers may only cancel `Scheduled` appointments (`403` for `InProgress`)

## Schema

Migration `AddAppointmentLifecycleFields` adds:

- `CancellationReason` (varchar 500)
- `StartedAtUtc`, `ClosedAtUtc` (timestamptz) — `ClosedAtUtc` set when status becomes `Completed` or `Cancelled`

## Key files

- `Infrastructure/Appointments/AppointmentStatusTransitions.cs`
- `Infrastructure/Appointments/AppointmentLifecycleRules.cs`
- `Infrastructure/Services/AppointmentService.cs` — `UpdateStatusAsync`, `CancelAsync`
- `Controllers/AppointmentController.cs`

## Availability impact

`AvailabilityEngine.IsBlockingStatus` excludes `Completed` and `Cancelled` — freed slots become bookable after cancel/complete.
