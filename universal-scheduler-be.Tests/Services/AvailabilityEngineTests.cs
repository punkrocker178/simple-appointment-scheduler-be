using Infrastructure.Appointments;
using Infrastructure.Entities;

namespace universal_scheduler_be.Tests.Services;

public class AvailabilityEngineTests
{
    private static readonly DateOnly BookingDate = new(2026, 6, 17);
    private static readonly DateTime UtcNow = new(2026, 6, 17, 8, 0, 0, DateTimeKind.Utc);
    private static readonly Guid SkillId = Guid.Parse("d4000001-0000-4000-8000-000000000001");
    private static readonly Guid TechId = Guid.Parse("e5000001-0000-4000-8000-000000000001");
    private static readonly Guid BayId = Guid.Parse("f6000001-0000-4000-8000-000000000001");

    [Fact]
    public void GetAvailableSlots_GeneratesSlotsEveryThirtyMinutes()
    {
        var technicians = new List<AvailabilityTechnician>
        {
            new(TechId, "Alex", "Tech", new HashSet<Guid> { SkillId })
        };
        var bays = new List<AvailabilityBay> { new(BayId, "Bay 1") };

        var slots = AvailabilityEngine.GetAvailableSlots(
            openSeconds: 28_800,
            closeSeconds: 61_200,
            durationMinutes: 60,
            requiredSkillId: SkillId,
            technicians,
            bays,
            appointments: [],
            BookingDate,
            UtcNow);

        Assert.Equal(
            [28_800, 30_600, 32_400, 34_200, 36_000, 37_800, 39_600, 41_400, 43_200, 45_000, 46_800, 48_600, 50_400, 52_200, 54_000, 55_800, 57_600],
            slots);
    }

    [Fact]
    public void GetAvailableSlots_ExcludesSlotSpanningPastClose()
    {
        var technicians = new List<AvailabilityTechnician>
        {
            new(TechId, "Alex", "Tech", new HashSet<Guid> { SkillId })
        };
        var bays = new List<AvailabilityBay> { new(BayId, "Bay 1") };

        var slots = AvailabilityEngine.GetAvailableSlots(
            openSeconds: 28_800,
            closeSeconds: 61_200,
            durationMinutes: 120,
            requiredSkillId: SkillId,
            technicians,
            bays,
            appointments: [],
            BookingDate,
            UtcNow);

        Assert.DoesNotContain(55_800, slots);
        Assert.Equal(54_000, slots[^1]);
    }

    [Fact]
    public void GetAvailableSlots_NoQualifiedTechnicians_ReturnsEmpty()
    {
        var technicians = new List<AvailabilityTechnician>
        {
            new(TechId, "Alex", "Tech", new HashSet<Guid>())
        };
        var bays = new List<AvailabilityBay> { new(BayId, "Bay 1") };

        var slots = AvailabilityEngine.GetAvailableSlots(
            28_800, 61_200, 60, SkillId, technicians, bays, [], BookingDate, UtcNow);

        Assert.Empty(slots);
    }

    [Fact]
    public void GetAvailableSlots_NoBays_ReturnsEmpty()
    {
        var technicians = new List<AvailabilityTechnician>
        {
            new(TechId, "Alex", "Tech", new HashSet<Guid> { SkillId })
        };

        var slots = AvailabilityEngine.GetAvailableSlots(
            28_800, 61_200, 60, SkillId, technicians, [], [], BookingDate, UtcNow);

        Assert.Empty(slots);
    }

    [Fact]
    public void GetAvailableSlots_BlockedTechnician_RemovesOverlappingSlots()
    {
        var technicians = new List<AvailabilityTechnician>
        {
            new(TechId, "Alex", "Tech", new HashSet<Guid> { SkillId })
        };
        var bays = new List<AvailabilityBay> { new(BayId, "Bay 1") };
        var appointments = new List<ExistingAppointmentRecord>
        {
            new(TechId, BayId, 28_800, 3600, AppointmentStatus.Scheduled)
        };

        var slots = AvailabilityEngine.GetAvailableSlots(
            28_800, 61_200, 60, SkillId, technicians, bays, appointments, BookingDate, UtcNow);

        Assert.DoesNotContain(28_800, slots);
        Assert.Contains(32_400, slots);
    }

    [Fact]
    public void GetAvailableSlots_RequiresBothTechnicianAndBayFree()
    {
        var tech2Id = Guid.Parse("e5000001-0000-4000-8000-000000000002");
        var bay2Id = Guid.Parse("f6000001-0000-4000-8000-000000000002");
        var technicians = new List<AvailabilityTechnician>
        {
            new(TechId, "Alex", "Tech", new HashSet<Guid> { SkillId }),
            new(tech2Id, "Blake", "Tech", new HashSet<Guid> { SkillId })
        };
        var bays = new List<AvailabilityBay>
        {
            new(BayId, "Bay 1"),
            new(bay2Id, "Bay 2")
        };
        var appointments = new List<ExistingAppointmentRecord>
        {
            new(TechId, BayId, 28_800, 3600, AppointmentStatus.Scheduled),
            new(tech2Id, bay2Id, 28_800, 3600, AppointmentStatus.Scheduled)
        };

        var slots = AvailabilityEngine.GetAvailableSlots(
            28_800, 61_200, 60, SkillId, technicians, bays, appointments, BookingDate, UtcNow);

        Assert.DoesNotContain(28_800, slots);
    }

    [Fact]
    public void GetAvailableSlots_ExcludesPastSlotsOnToday()
    {
        var today = DateOnly.FromDateTime(UtcNow);
        var laterToday = new DateTime(2026, 6, 17, 9, 0, 0, DateTimeKind.Utc);
        var technicians = new List<AvailabilityTechnician>
        {
            new(TechId, "Alex", "Tech", new HashSet<Guid> { SkillId })
        };
        var bays = new List<AvailabilityBay> { new(BayId, "Bay 1") };

        var slots = AvailabilityEngine.GetAvailableSlots(
            28_800, 61_200, 60, SkillId, technicians, bays, [], today, laterToday);

        Assert.DoesNotContain(28_800, slots);
        Assert.Contains(32_400, slots);
    }

    [Fact]
    public void IntervalsOverlap_DetectsPartialOverlap()
    {
        Assert.True(AvailabilityEngine.IntervalsOverlap(28_800, 32_400, 30_600, 34_200));
        Assert.False(AvailabilityEngine.IntervalsOverlap(28_800, 30_600, 30_600, 32_400));
    }

    [Fact]
    public void ValidateBusinessHours_RejectsInvalidRange()
    {
        Assert.Equal("Open time must be before close time.", AvailabilityEngine.ValidateBusinessHours(61_200, 28_800));
        Assert.Equal("Business hours must align to 30-minute increments.", AvailabilityEngine.ValidateBusinessHours(28_100, 61_200));
    }
}
