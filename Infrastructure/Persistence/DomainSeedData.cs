using Infrastructure.Entities;

namespace Infrastructure.Persistence;

public static class DomainSeedData
{
    public static class DealershipIds
    {
        public static readonly Guid Main = Guid.Parse("c3000001-0000-4000-8000-000000000001");
    }

    public static class SkillIds
    {
        public static readonly Guid Basic = Guid.Parse("c3000002-0000-4000-8000-000000000001");
        public static readonly Guid Intermediate = Guid.Parse("c3000002-0000-4000-8000-000000000002");
        public static readonly Guid Advanced = Guid.Parse("c3000002-0000-4000-8000-000000000003");
    }

    public static class ServiceTypeIds
    {
        public static readonly Guid OilChange = Guid.Parse("c3000003-0000-4000-8000-000000000001");
        public static readonly Guid FullService = Guid.Parse("c3000003-0000-4000-8000-000000000002");
        public static readonly Guid MajorService = Guid.Parse("c3000003-0000-4000-8000-000000000003");
    }

    public static class ServiceBayIds
    {
        public static readonly Guid Bay1 = Guid.Parse("c3000004-0000-4000-8000-000000000001");
        public static readonly Guid Bay2 = Guid.Parse("c3000004-0000-4000-8000-000000000002");
        public static readonly Guid Bay3 = Guid.Parse("c3000004-0000-4000-8000-000000000003");
    }

    public static Dealership Dealership { get; } = new()
    {
        Id = DealershipIds.Main,
        Name = "Universal Auto Service",
        Address = "123 Main Street, Austin, TX 78701",
        Phone = "+1-512-555-0100",
        Timezone = "America/Chicago"
    };

    public static IEnumerable<Skill> Skills =>
    [
        new Skill
        {
            Id = SkillIds.Basic,
            Name = "Basic",
            Description = "Routine maintenance such as oil changes"
        },
        new Skill
        {
            Id = SkillIds.Intermediate,
            Name = "Intermediate",
            Description = "Standard multi-point inspections and full service"
        },
        new Skill
        {
            Id = SkillIds.Advanced,
            Name = "Advanced",
            Description = "Complex diagnostics and major service work"
        }
    ];

    public static IEnumerable<ServiceType> ServiceTypes =>
    [
        new ServiceType
        {
            Id = ServiceTypeIds.OilChange,
            DealershipId = DealershipIds.Main,
            SkillId = SkillIds.Basic,
            Name = "Oil Change",
            Description = "Standard oil and filter change",
            DurationMinutes = 60,
            Price = 49.99m,
            IsActive = true
        },
        new ServiceType
        {
            Id = ServiceTypeIds.FullService,
            DealershipId = DealershipIds.Main,
            SkillId = SkillIds.Intermediate,
            Name = "Full Service",
            Description = "Multi-point inspection with fluids and filters",
            DurationMinutes = 120,
            Price = 149.99m,
            IsActive = true
        },
        new ServiceType
        {
            Id = ServiceTypeIds.MajorService,
            DealershipId = DealershipIds.Main,
            SkillId = SkillIds.Advanced,
            Name = "Major Service",
            Description = "Comprehensive maintenance and diagnostics",
            DurationMinutes = 180,
            Price = 299.99m,
            IsActive = true
        }
    ];

    public static IEnumerable<ServiceBay> ServiceBays =>
    [
        new ServiceBay
        {
            Id = ServiceBayIds.Bay1,
            DealershipId = DealershipIds.Main,
            Name = "Bay 1",
            IsActive = true
        },
        new ServiceBay
        {
            Id = ServiceBayIds.Bay2,
            DealershipId = DealershipIds.Main,
            Name = "Bay 2",
            IsActive = true
        },
        new ServiceBay
        {
            Id = ServiceBayIds.Bay3,
            DealershipId = DealershipIds.Main,
            Name = "Bay 3",
            IsActive = true
        }
    ];
}
