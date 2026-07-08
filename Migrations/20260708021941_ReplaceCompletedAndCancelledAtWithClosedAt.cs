using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace universal_scheduler_be.Migrations
{
    /// <inheritdoc />
    public partial class ReplaceCompletedAndCancelledAtWithClosedAt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "CompletedAtUtc",
                table: "Appointments",
                newName: "ClosedAtUtc");

            migrationBuilder.Sql(
                """
                UPDATE "Appointments"
                SET "ClosedAtUtc" = "CancelledAtUtc"
                WHERE "ClosedAtUtc" IS NULL AND "CancelledAtUtc" IS NOT NULL;
                """);

            migrationBuilder.DropColumn(
                name: "CancelledAtUtc",
                table: "Appointments");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CancelledAtUtc",
                table: "Appointments",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.Sql(
                """
                UPDATE "Appointments"
                SET "CancelledAtUtc" = "ClosedAtUtc"
                WHERE "Status" = 3 AND "ClosedAtUtc" IS NOT NULL;
                """);

            migrationBuilder.Sql(
                """
                UPDATE "Appointments"
                SET "ClosedAtUtc" = NULL
                WHERE "Status" = 3;
                """);

            migrationBuilder.RenameColumn(
                name: "ClosedAtUtc",
                table: "Appointments",
                newName: "CompletedAtUtc");
        }
    }
}
