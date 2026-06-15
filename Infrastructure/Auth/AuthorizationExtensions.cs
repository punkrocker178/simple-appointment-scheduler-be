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
    }
}
