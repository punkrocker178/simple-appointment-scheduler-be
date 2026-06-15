using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace universal_scheduler_be.Migrations
{
    /// <inheritdoc />
    public partial class AddSkillAndServiceTypePermissions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Permissions",
                columns: new[] { "Id", "Description", "Name" },
                values: new object[,]
                {
                    { new Guid("a1000001-0000-4000-8000-000000000007"), "View skills (admin only)", "skills:read" },
                    { new Guid("a1000001-0000-4000-8000-000000000008"), "Create and delete skills (admin only)", "skills:write" },
                    { new Guid("a1000001-0000-4000-8000-000000000009"), "View service types (admin only)", "servicetypes:read" },
                    { new Guid("a1000001-0000-4000-8000-00000000000a"), "Create, update, and deactivate service types (admin only)", "servicetypes:write" }
                });

            migrationBuilder.InsertData(
                table: "RolePermissions",
                columns: new[] { "PermissionId", "RoleId" },
                values: new object[,]
                {
                    { new Guid("a1000001-0000-4000-8000-000000000007"), new Guid("b2000001-0000-4000-8000-000000000001") },
                    { new Guid("a1000001-0000-4000-8000-000000000008"), new Guid("b2000001-0000-4000-8000-000000000001") },
                    { new Guid("a1000001-0000-4000-8000-000000000009"), new Guid("b2000001-0000-4000-8000-000000000001") },
                    { new Guid("a1000001-0000-4000-8000-00000000000a"), new Guid("b2000001-0000-4000-8000-000000000001") }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("a1000001-0000-4000-8000-000000000007"), new Guid("b2000001-0000-4000-8000-000000000001") });

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("a1000001-0000-4000-8000-000000000008"), new Guid("b2000001-0000-4000-8000-000000000001") });

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("a1000001-0000-4000-8000-000000000009"), new Guid("b2000001-0000-4000-8000-000000000001") });

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("a1000001-0000-4000-8000-00000000000a"), new Guid("b2000001-0000-4000-8000-000000000001") });

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("a1000001-0000-4000-8000-000000000007"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("a1000001-0000-4000-8000-000000000008"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("a1000001-0000-4000-8000-000000000009"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("a1000001-0000-4000-8000-00000000000a"));
        }
    }
}
