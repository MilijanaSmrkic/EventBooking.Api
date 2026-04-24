using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EventBooking.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddReservationAuditFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CancelledAt",
                table: "Reservations",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CancelledByUserId",
                table: "Reservations",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifiedAt",
                table: "Reservations",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LastModifiedByUserId",
                table: "Reservations",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CancelledAt",
                table: "Reservations");

            migrationBuilder.DropColumn(
                name: "CancelledByUserId",
                table: "Reservations");

            migrationBuilder.DropColumn(
                name: "LastModifiedAt",
                table: "Reservations");

            migrationBuilder.DropColumn(
                name: "LastModifiedByUserId",
                table: "Reservations");
        }
    }
}
