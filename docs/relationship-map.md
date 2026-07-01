Customer ──< Vehicle
Customer ──< Appointment

Dealership ──< ServiceBay
Dealership ──< Technician
Dealership ──< ServiceType
Dealership.OpenSecondsFromMidnight / CloseSecondsFromMidnight  (fixed business hours, UTC)

Appointment >── Vehicle          (which car is being serviced)
Appointment >── Technician       (who does the work)
Appointment >── ServiceBay       (where the work happens)
Appointment >── ServiceType      (what work is being done)
Appointment ──< AppointmentStatusLog   (audit trail)
Appointment ──< Notification     (emails, SMS)

Technician ──< AvailabilityRule       (recurring weekly schedule) ↪ deferred — use Dealership hours for Phase 4
Technician ──< AvailabilityOverride   (sick days, holidays)       ↪ deferred — use Dealership hours for Phase 4