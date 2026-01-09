using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pengguna.Migrations
{
    /// <inheritdoc />
    public partial class tou : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Garansis",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NomorServiceRef = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NamaCustomer = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Keluhan = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BuktiFotoPath = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TanggalKlaim = table.Column<DateTime>(type: "datetime2", nullable: false),
                    StatusKlaim = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CatatanAdmin = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Garansis", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Garansis");
        }
    }
}
