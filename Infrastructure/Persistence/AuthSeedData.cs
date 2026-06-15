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
        new RolePermission { RoleId = RoleIds.Staff, PermissionId = PermissionIds.AppointmentsRead },
        new RolePermission { RoleId = RoleIds.Staff, PermissionId = PermissionIds.AppointmentsWrite },
        new RolePermission { RoleId = RoleIds.User, PermissionId = PermissionIds.AppointmentsReadOwn },
        new RolePermission { RoleId = RoleIds.User, PermissionId = PermissionIds.AppointmentsWrite }
    ];
}
