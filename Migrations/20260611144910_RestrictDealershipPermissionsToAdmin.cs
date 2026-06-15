using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace universal_scheduler_be.Migrations
{
    /// <inheritdoc />
    public partial class RestrictDealershipPermissionsToAdmin : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("a1000001-0000-4000-8000-000000000005"), new Guid("b2000001-0000-4000-8000-000000000002") });

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("a1000001-0000-4000-8000-000000000006"), new Guid("b2000001-0000-4000-8000-000000000002") });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("a1000001-0000-4000-8000-000000000005"),
                column: "Description",
                value: "View dealerships (admin only)");

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("a1000001-0000-4000-8000-000000000006"),
                column: "Description",
                value: "Create and update dealerships (admin only)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("a1000001-0000-4000-8000-000000000005"),
                column: "Description",
                value: "View dealerships");

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("a1000001-0000-4000-8000-000000000006"),
                column: "Description",
                value: "Create and update dealerships");

            migrationBuilder.InsertData(
                table: "RolePermissions",
                columns: new[] { "PermissionId", "RoleId" },
                values: new object[,]
                {
                    { new Guid("a1000001-0000-4000-8000-000000000005"), new Guid("b2000001-0000-4000-8000-000000000002") },
                    { new Guid("a1000001-0000-4000-8000-000000000006"), new Guid("b2000001-0000-4000-8000-000000000002") }
                });
        }
    }
}
