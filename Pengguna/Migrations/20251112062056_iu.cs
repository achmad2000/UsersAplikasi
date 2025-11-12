using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pengguna.Migrations
{
    /// <inheritdoc />
    public partial class iu : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CancelRequestedAt",
                table: "WaitingResponOrders",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "WaitingResponOrders",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "TechnicianId",
                table: "WaitingResponOrders",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CancelRequestedAt",
                table: "WaitingResponOrders");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "WaitingResponOrders");

            migrationBuilder.DropColumn(
                name: "TechnicianId",
                table: "WaitingResponOrders");
        }
    }
}
