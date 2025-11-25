using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pengguna.Migrations
{
    /// <inheritdoc />
    public partial class rtyu : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BuktiPembayaranPath",
                table: "ServiceLogs",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BuktiPembayaranPath",
                table: "ServiceLogs");
        }
    }
}
