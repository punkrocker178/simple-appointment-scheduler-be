using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace universal_scheduler_be.Migrations
{
    /// <inheritdoc />
    public partial class Phase4AppointmentBooking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EndTime",
                table: "Appointments");

            migrationBuilder.DropColumn(
                name: "StartTime",
                table: "Appointments");

            migrationBuilder.AddColumn<int>(
                name: "CloseSecondsFromMidnight",
                table: "Dealerships",
                type: "integer",
                nullable: false,
                defaultValue: 61200);

            migrationBuilder.AddColumn<int>(
                name: "OpenSecondsFromMidnight",
                table: "Dealerships",
                type: "integer",
                nullable: false,
                defaultValue: 28800);

            migrationBuilder.AddColumn<DateOnly>(
                name: "BookingDate",
                table: "Appointments",
                type: "date",
                nullable: false,
                defaultValue: new DateOnly(1, 1, 1));

            migrationBuilder.AddColumn<int>(
                name: "SecondsFromMidnight",
                table: "Appointments",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Appointments",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_BookingDate_ServiceBayId",
                table: "Appointments",
                columns: new[] { "BookingDate", "ServiceBayId" });

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_BookingDate_TechnicianId",
                table: "Appointments",
                columns: new[] { "BookingDate", "TechnicianId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Appointments_BookingDate_ServiceBayId",
                table: "Appointments");

            migrationBuilder.DropIndex(
                name: "IX_Appointments_BookingDate_TechnicianId",
                table: "Appointments");

            migrationBuilder.DropColumn(
                name: "CloseSecondsFromMidnight",
                table: "Dealerships");

            migrationBuilder.DropColumn(
                name: "OpenSecondsFromMidnight",
                table: "Dealerships");

            migrationBuilder.DropColumn(
                name: "BookingDate",
                table: "Appointments");

            migrationBuilder.DropColumn(
                name: "SecondsFromMidnight",
                table: "Appointments");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Appointments");

            migrationBuilder.AddColumn<DateTime>(
                name: "EndTime",
                table: "Appointments",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "StartTime",
                table: "Appointments",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }
    }
}
