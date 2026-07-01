using Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class AppointmentConfiguration : IEntityTypeConfiguration<Appointment>
{
    public void Configure(EntityTypeBuilder<Appointment> builder)
    {
        builder.HasKey(a => a.Id);

        builder.Property(a => a.BookingDate)
            .HasColumnType("date");

        builder.Property(a => a.Status)
            .HasConversion<int>()
            .HasDefaultValue(AppointmentStatus.Scheduled);

        builder.HasIndex(a => new { a.BookingDate, a.TechnicianId });
        builder.HasIndex(a => new { a.BookingDate, a.ServiceBayId });

        builder.HasOne(a => a.Customer)
            .WithMany(c => c.Appointments)
            .HasForeignKey(a => a.CustomerId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(a => a.Vehicle)
            .WithMany(v => v.Appointments)
            .HasForeignKey(a => a.VehicleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(a => a.Technician)
            .WithMany(t => t.Appointments)
            .HasForeignKey(a => a.TechnicianId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(a => a.ServiceBay)
            .WithMany(b => b.Appointments)
            .HasForeignKey(a => a.ServiceBayId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(a => a.ServiceType)
            .WithMany(st => st.Appointments)
            .HasForeignKey(a => a.ServiceTypeId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
