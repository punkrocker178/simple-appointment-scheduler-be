Customer ──< Vehicle
Customer ──< Appointment

Dealership ──< ServiceBay
Dealership ──< Technician
Dealership ──< ServiceType

Appointment >── Vehicle          (which car is being serviced)
Appointment >── Technician       (who does the work)
Appointment >── ServiceBay       (where the work happens)
Appointment >── ServiceType      (what work is being done)
Appointment ──< AppointmentStatusLog   (audit trail)
Appointment ──< Notification     (emails, SMS)

Technician ──< AvailabilityRule       (recurring weekly schedule)
Technician ──< AvailabilityOverride   (sick days, holidays)