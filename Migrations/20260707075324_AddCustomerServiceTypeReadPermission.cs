using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace universal_scheduler_be.Migrations
{
    /// <inheritdoc />
    public partial class AddCustomerServiceTypeReadPermission : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Permissions",
                columns: new[] { "Id", "Description", "Name" },
                values: new object[] { new Guid("a1000001-0000-4000-8000-000000000013"), "View service types as a customer", "servicetypes:read:customer" });

            migrationBuilder.InsertData(
                table: "RolePermissions",
                columns: new[] { "PermissionId", "RoleId" },
                values: new object[] { new Guid("a1000001-0000-4000-8000-000000000013"), new Guid("b2000001-0000-4000-8000-000000000003") });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("a1000001-0000-4000-8000-000000000013"), new Guid("b2000001-0000-4000-8000-000000000003") });

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("a1000001-0000-4000-8000-000000000013"));
        }
    }
}
