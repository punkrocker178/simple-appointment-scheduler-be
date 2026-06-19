using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace universal_scheduler_be.Migrations
{
    /// <inheritdoc />
    public partial class AddServiceBayAndTechnicianPermissions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Permissions",
                columns: new[] { "Id", "Description", "Name" },
                values: new object[,]
                {
                    { new Guid("a1000001-0000-4000-8000-00000000000b"), "View service bays (admin only)", "servicebays:read" },
                    { new Guid("a1000001-0000-4000-8000-00000000000c"), "Create, update, and deactivate service bays (admin only)", "servicebays:write" },
                    { new Guid("a1000001-0000-4000-8000-00000000000d"), "View technicians (admin only)", "technicians:read" },
                    { new Guid("a1000001-0000-4000-8000-00000000000e"), "Create, update, and deactivate technicians (admin only)", "technicians:write" }
                });

            migrationBuilder.InsertData(
                table: "RolePermissions",
                columns: new[] { "PermissionId", "RoleId" },
                values: new object[,]
                {
                    { new Guid("a1000001-0000-4000-8000-00000000000b"), new Guid("b2000001-0000-4000-8000-000000000001") },
                    { new Guid("a1000001-0000-4000-8000-00000000000c"), new Guid("b2000001-0000-4000-8000-000000000001") },
                    { new Guid("a1000001-0000-4000-8000-00000000000d"), new Guid("b2000001-0000-4000-8000-000000000001") },
                    { new Guid("a1000001-0000-4000-8000-00000000000e"), new Guid("b2000001-0000-4000-8000-000000000001") }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("a1000001-0000-4000-8000-00000000000b"), new Guid("b2000001-0000-4000-8000-000000000001") });

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("a1000001-0000-4000-8000-00000000000c"), new Guid("b2000001-0000-4000-8000-000000000001") });

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("a1000001-0000-4000-8000-00000000000d"), new Guid("b2000001-0000-4000-8000-000000000001") });

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("a1000001-0000-4000-8000-00000000000e"), new Guid("b2000001-0000-4000-8000-000000000001") });

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("a1000001-0000-4000-8000-00000000000b"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("a1000001-0000-4000-8000-00000000000c"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("a1000001-0000-4000-8000-00000000000d"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("a1000001-0000-4000-8000-00000000000e"));
        }
    }
}
