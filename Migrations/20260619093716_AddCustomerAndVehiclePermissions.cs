using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace universal_scheduler_be.Migrations
{
    /// <inheritdoc />
    public partial class AddCustomerAndVehiclePermissions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Permissions",
                columns: new[] { "Id", "Description", "Name" },
                values: new object[,]
                {
                    { new Guid("a1000001-0000-4000-8000-00000000000f"), "View customers (admin and staff)", "customers:read" },
                    { new Guid("a1000001-0000-4000-8000-000000000010"), "Create and update customers (admin and staff)", "customers:write" },
                    { new Guid("a1000001-0000-4000-8000-000000000011"), "View vehicles (admin and staff)", "vehicles:read" },
                    { new Guid("a1000001-0000-4000-8000-000000000012"), "Create, update, and delete vehicles (admin and staff)", "vehicles:write" }
                });

            migrationBuilder.InsertData(
                table: "RolePermissions",
                columns: new[] { "PermissionId", "RoleId" },
                values: new object[,]
                {
                    { new Guid("a1000001-0000-4000-8000-00000000000f"), new Guid("b2000001-0000-4000-8000-000000000001") },
                    { new Guid("a1000001-0000-4000-8000-000000000010"), new Guid("b2000001-0000-4000-8000-000000000001") },
                    { new Guid("a1000001-0000-4000-8000-000000000011"), new Guid("b2000001-0000-4000-8000-000000000001") },
                    { new Guid("a1000001-0000-4000-8000-000000000012"), new Guid("b2000001-0000-4000-8000-000000000001") },
                    { new Guid("a1000001-0000-4000-8000-00000000000f"), new Guid("b2000001-0000-4000-8000-000000000002") },
                    { new Guid("a1000001-0000-4000-8000-000000000010"), new Guid("b2000001-0000-4000-8000-000000000002") },
                    { new Guid("a1000001-0000-4000-8000-000000000011"), new Guid("b2000001-0000-4000-8000-000000000002") },
                    { new Guid("a1000001-0000-4000-8000-000000000012"), new Guid("b2000001-0000-4000-8000-000000000002") }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("a1000001-0000-4000-8000-00000000000f"), new Guid("b2000001-0000-4000-8000-000000000001") });

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("a1000001-0000-4000-8000-000000000010"), new Guid("b2000001-0000-4000-8000-000000000001") });

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("a1000001-0000-4000-8000-000000000011"), new Guid("b2000001-0000-4000-8000-000000000001") });

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("a1000001-0000-4000-8000-000000000012"), new Guid("b2000001-0000-4000-8000-000000000001") });

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("a1000001-0000-4000-8000-00000000000f"), new Guid("b2000001-0000-4000-8000-000000000002") });

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("a1000001-0000-4000-8000-000000000010"), new Guid("b2000001-0000-4000-8000-000000000002") });

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("a1000001-0000-4000-8000-000000000011"), new Guid("b2000001-0000-4000-8000-000000000002") });

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("a1000001-0000-4000-8000-000000000012"), new Guid("b2000001-0000-4000-8000-000000000002") });

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("a1000001-0000-4000-8000-00000000000f"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("a1000001-0000-4000-8000-000000000010"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("a1000001-0000-4000-8000-000000000011"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("a1000001-0000-4000-8000-000000000012"));
        }
    }
}
