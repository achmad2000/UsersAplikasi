using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pengguna.Migrations
{
    /// <inheritdoc />
    public partial class ty : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "StatusKlaim",
                table: "Garansis",
                newName: "Status");

            migrationBuilder.RenameColumn(
                name: "Keluhan",
                table: "Garansis",
                newName: "ItemService");

            migrationBuilder.RenameColumn(
                name: "BuktiFotoPath",
                table: "Garansis",
                newName: "DeskripsiKeluhan");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Status",
                table: "Garansis",
                newName: "StatusKlaim");

            migrationBuilder.RenameColumn(
                name: "ItemService",
                table: "Garansis",
                newName: "Keluhan");

            migrationBuilder.RenameColumn(
                name: "DeskripsiKeluhan",
                table: "Garansis",
                newName: "BuktiFotoPath");
        }
    }
}
