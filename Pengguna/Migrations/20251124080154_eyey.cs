using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pengguna.Migrations
{
    /// <inheritdoc />
    public partial class eyey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SelesaiServices",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NomorService = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NamaCustomer = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ItemService = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    JadwalService = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NoWA = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Alamat = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DeskripsiProblem = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TanggalOrder = table.Column<DateTime>(type: "datetime2", nullable: false),
                    StatusPembayaran = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TechnicianId = table.Column<int>(type: "int", nullable: true),
                    NamaTeknisi = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    WaktuSelesai = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TotalBiaya = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SelesaiServices", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SelesaiServices");
        }
    }
}
