using Infrastructure.Entities;

namespace Infrastructure.Persistence;

public static class AuthSeedData
{
    public static class PermissionIds
    {
        public static readonly Guid AppointmentsRead = Guid.Parse("a1000001-0000-4000-8000-000000000001");
        public static readonly Guid AppointmentsReadOwn = Guid.Parse("a1000001-0000-4000-8000-000000000002");
        public static readonly Guid AppointmentsWrite = Guid.Parse("a1000001-0000-4000-8000-000000000003");
        public static readonly Guid UsersManage = Guid.Parse("a1000001-0000-4000-8000-000000000004");
        public static readonly Guid DealershipsRead = Guid.Parse("a1000001-0000-4000-8000-000000000005");
        public static readonly Guid DealershipsWrite = Guid.Parse("a1000001-0000-4000-8000-000000000006");
        public static readonly Guid SkillsRead = Guid.Parse("a1000001-0000-4000-8000-000000000007");
        public static readonly Guid SkillsWrite = Guid.Parse("a1000001-0000-4000-8000-000000000008");
        public static readonly Guid ServiceTypesRead = Guid.Parse("a1000001-0000-4000-8000-000000000009");
        public static readonly Guid ServiceTypesWrite = Guid.Parse("a1000001-0000-4000-8000-00000000000a");
        public static readonly Guid ServiceBaysRead = Guid.Parse("a1000001-0000-4000-8000-00000000000b");
        public static readonly Guid ServiceBaysWrite = Guid.Parse("a1000001-0000-4000-8000-00000000000c");
        public static readonly Guid TechniciansRead = Guid.Parse("a1000001-0000-4000-8000-00000000000d");
        public static readonly Guid TechniciansWrite = Guid.Parse("a1000001-0000-4000-8000-00000000000e");
        public static readonly Guid CustomersRead = Guid.Parse("a1000001-0000-4000-8000-00000000000f");
        public static readonly Guid CustomersWrite = Guid.Parse("a1000001-0000-4000-8000-000000000010");
        public static readonly Guid VehiclesRead = Guid.Parse("a1000001-0000-4000-8000-000000000011");
        public static readonly Guid VehiclesWrite = Guid.Parse("a1000001-0000-4000-8000-000000000012");
    }

    public static class RoleIds
    {
        public static readonly Guid Admin = Guid.Parse("b2000001-0000-4000-8000-000000000001");
        public static readonly Guid Staff = Guid.Parse("b2000001-0000-4000-8000-000000000002");
        public static readonly Guid User = Guid.Parse("b2000001-0000-4000-8000-000000000003");
    }

    public static IEnumerable<Permission> Permissions =>
    [
        new Permission
        {
            Id = PermissionIds.AppointmentsRead,
            Name = "appointments:read",
            Description = "View all appointments (staff/admin)"
        },
        new Permission
        {
            Id = PermissionIds.AppointmentsReadOwn,
            Name = "appointments:read:own",
            Description = "View only the signed-in user's appointments"
        },
        new Permission
        {
            Id = PermissionIds.AppointmentsWrite,
            Name = "appointments:write",
            Description = "Create and update appointments"
        },
        new Permission
        {
            Id = PermissionIds.UsersManage,
            Name = "users:manage",
            Description = "Manage users and roles"
        },
        new Permission
        {
            Id = PermissionIds.DealershipsRead,
            Name = "dealerships:read",
            Description = "View dealerships (admin only)"
        },
        new Permission
        {
            Id = PermissionIds.DealershipsWrite,
            Name = "dealerships:write",
            Description = "Create and update dealerships (admin only)"
        },
        new Permission
        {
            Id = PermissionIds.SkillsRead,
            Name = "skills:read",
            Description = "View skills (admin only)"
        },
        new Permission
        {
            Id = PermissionIds.SkillsWrite,
            Name = "skills:write",
            Description = "Create and delete skills (admin only)"
        },
        new Permission
        {
            Id = PermissionIds.ServiceTypesRead,
            Name = "servicetypes:read",
            Description = "View service types (admin only)"
        },
        new Permission
        {
            Id = PermissionIds.ServiceTypesWrite,
            Name = "servicetypes:write",
            Description = "Create, update, and deactivate service types (admin only)"
        },
        new Permission
        {
            Id = PermissionIds.ServiceBaysRead,
            Name = "servicebays:read",
            Description = "View service bays (admin only)"
        },
        new Permission
        {
            Id = PermissionIds.ServiceBaysWrite,
            Name = "servicebays:write",
            Description = "Create, update, and deactivate service bays (admin only)"
        },
        new Permission
        {
            Id = PermissionIds.TechniciansRead,
            Name = "technicians:read",
            Description = "View technicians (admin only)"
        },
        new Permission
        {
            Id = PermissionIds.TechniciansWrite,
            Name = "technicians:write",
            Description = "Create, update, and deactivate technicians (admin only)"
        },
        new Permission
        {
            Id = PermissionIds.CustomersRead,
            Name = "customers:read",
            Description = "View customers (admin and staff)"
        },
        new Permission
        {
            Id = PermissionIds.CustomersWrite,
            Name = "customers:write",
            Description = "Create and update customers (admin and staff)"
        },
        new Permission
        {
            Id = PermissionIds.VehiclesRead,
            Name = "vehicles:read",
            Description = "View vehicles (admin and staff)"
        },
        new Permission
        {
            Id = PermissionIds.VehiclesWrite,
            Name = "vehicles:write",
            Description = "Create, update, and delete vehicles (admin and staff)"
        }
    ];

    public static IEnumerable<Role> Roles =>
    [
        new Role
        {
            Id = RoleIds.Admin,
            Name = "Admin",
            Description = "Full system access"
        },
        new Role
        {
            Id = RoleIds.Staff,
            Name = "Staff",
            Description = "Day-to-day scheduling (all appointments)"
        },
        new Role
        {
            Id = RoleIds.User,
            Name = "User",
            Description = "Self-service: own appointments only"
        }
    ];

    public static IEnumerable<RolePermission> RolePermissions =>
    [
        new RolePermission { RoleId = RoleIds.Admin, PermissionId = PermissionIds.AppointmentsRead },
        new RolePermission { RoleId = RoleIds.Admin, PermissionId = PermissionIds.AppointmentsReadOwn },
        new RolePermission { RoleId = RoleIds.Admin, PermissionId = PermissionIds.AppointmentsWrite },
        new RolePermission { RoleId = RoleIds.Admin, PermissionId = PermissionIds.UsersManage },
        new RolePermission { RoleId = RoleIds.Admin, PermissionId = PermissionIds.DealershipsRead },
        new RolePermission { RoleId = RoleIds.Admin, PermissionId = PermissionIds.DealershipsWrite },
        new RolePermission { RoleId = RoleIds.Admin, PermissionId = PermissionIds.SkillsRead },
        new RolePermission { RoleId = RoleIds.Admin, PermissionId = PermissionIds.SkillsWrite },
        new RolePermission { RoleId = RoleIds.Admin, PermissionId = PermissionIds.ServiceTypesRead },
        new RolePermission { RoleId = RoleIds.Admin, PermissionId = PermissionIds.ServiceTypesWrite },
        new RolePermission { RoleId = RoleIds.Admin, PermissionId = PermissionIds.ServiceBaysRead },
        new RolePermission { RoleId = RoleIds.Admin, PermissionId = PermissionIds.ServiceBaysWrite },
        new RolePermission { RoleId = RoleIds.Admin, PermissionId = PermissionIds.TechniciansRead },
        new RolePermission { RoleId = RoleIds.Admin, PermissionId = PermissionIds.TechniciansWrite },
        new RolePermission { RoleId = RoleIds.Admin, PermissionId = PermissionIds.CustomersRead },
        new RolePermission { RoleId = RoleIds.Admin, PermissionId = PermissionIds.CustomersWrite },
        new RolePermission { RoleId = RoleIds.Admin, PermissionId = PermissionIds.VehiclesRead },
        new RolePermission { RoleId = RoleIds.Admin, PermissionId = PermissionIds.VehiclesWrite },
        new RolePermission { RoleId = RoleIds.Staff, PermissionId = PermissionIds.CustomersRead },
        new RolePermission { RoleId = RoleIds.Staff, PermissionId = PermissionIds.CustomersWrite },
        new RolePermission { RoleId = RoleIds.Staff, PermissionId = PermissionIds.VehiclesRead },
        new RolePermission { RoleId = RoleIds.Staff, PermissionId = PermissionIds.VehiclesWrite },
        new RolePermission { RoleId = RoleIds.Staff, PermissionId = PermissionIds.AppointmentsRead },
        new RolePermission { RoleId = RoleIds.Staff, PermissionId = PermissionIds.AppointmentsWrite },
        new RolePermission { RoleId = RoleIds.User, PermissionId = PermissionIds.AppointmentsReadOwn },
        new RolePermission { RoleId = RoleIds.User, PermissionId = PermissionIds.AppointmentsWrite }
    ];
}
