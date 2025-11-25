using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pengguna.Migrations
{
    /// <inheritdoc />
    public partial class RemoveWaitingResponOrders : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "NomorService",
                table: "WaitingResponOrders",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "JenisService",
                table: "ServiceLogDetails",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NomorService",
                table: "WaitingResponOrders");

            migrationBuilder.DropColumn(
                name: "JenisService",
                table: "ServiceLogDetails");
        }
    }
}
