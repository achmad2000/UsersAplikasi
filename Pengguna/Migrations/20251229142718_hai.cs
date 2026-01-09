using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pengguna.Migrations
{
    /// <inheritdoc />
    public partial class hai : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Rating",
                table: "SelesaiServices",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Ulasan",
                table: "SelesaiServices",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Rating",
                table: "SelesaiServices");

            migrationBuilder.DropColumn(
                name: "Ulasan",
                table: "SelesaiServices");
        }
    }
}
