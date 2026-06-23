using Microsoft.AspNetCore.Authorization;

namespace Infrastructure.Auth;

public static class AuthorizationExtensions
{
    public static void AddPermissionPolicies(this AuthorizationOptions options)
    {
        options.AddPolicy("appointments:read", policy =>
            policy.RequireClaim("permission", "appointments:read"));

        options.AddPolicy("appointments:read:own", policy =>
            policy.RequireClaim("permission", "appointments:read:own"));

        options.AddPolicy("appointments:write", policy =>
            policy.RequireClaim("permission", "appointments:write"));

        options.AddPolicy("users:manage", policy =>
            policy.RequireClaim("permission", "users:manage"));

        options.AddPolicy("dealerships:read", policy =>
            policy.RequireClaim("permission", "dealerships:read"));

        options.AddPolicy("dealerships:write", policy =>
            policy.RequireClaim("permission", "dealerships:write"));

        options.AddPolicy("skills:read", policy =>
            policy.RequireClaim("permission", "skills:read"));

        options.AddPolicy("skills:write", policy =>
            policy.RequireClaim("permission", "skills:write"));

        options.AddPolicy("servicetypes:read", policy =>
            policy.RequireClaim("permission", "servicetypes:read"));

        options.AddPolicy("servicetypes:write", policy =>
            policy.RequireClaim("permission", "servicetypes:write"));

        options.AddPolicy("servicebays:read", policy =>
            policy.RequireClaim("permission", "servicebays:read"));

        options.AddPolicy("servicebays:write", policy =>
            policy.RequireClaim("permission", "servicebays:write"));

        options.AddPolicy("technicians:read", policy =>
            policy.RequireClaim("permission", "technicians:read"));

        options.AddPolicy("technicians:write", policy =>
            policy.RequireClaim("permission", "technicians:write"));

        options.AddPolicy("customers:read", policy =>
            policy.RequireClaim("permission", "customers:read"));

        options.AddPolicy("customers:write", policy =>
            policy.RequireClaim("permission", "customers:write"));

        options.AddPolicy("vehicles:read", policy =>
            policy.RequireClaim("permission", "vehicles:read"));

        options.AddPolicy("vehicles:write", policy =>
            policy.RequireClaim("permission", "vehicles:write"));
    }
}
