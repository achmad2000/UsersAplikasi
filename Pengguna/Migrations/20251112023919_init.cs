using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pengguna.Migrations
{
    /// <inheritdoc />
    public partial class init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ActiveOrders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NamaCustomer = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ItemService = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NoWA = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Alamat = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DeskripsiProblem = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TeknisiNama = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TanggalAmbil = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActiveOrders", x => x.Id);
                });

            //migrationBuilder.CreateTable(
            //    name: "AdminNets",
            //    columns: table => new
            //    {
            //        Id = table.Column<int>(type: "int", nullable: false)
            //            .Annotation("SqlServer:Identity", "1, 1"),
            //        Username = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_AdminNets", x => x.Id);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "Technicians",
            //    columns: table => new
            //    {
            //        Id = table.Column<int>(type: "int", nullable: false)
            //            .Annotation("SqlServer:Identity", "1, 1"),
            //        FullName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
            //        Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        Phone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
            //        Username = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
            //        Password = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_Technicians", x => x.Id);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "Users",
            //    columns: table => new
            //    {
            //        Id = table.Column<int>(type: "int", nullable: false)
            //            .Annotation("SqlServer:Identity", "1, 1"),
            //        Username = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        Role = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        ProfileImagePath = table.Column<string>(type: "nvarchar(max)", nullable: true)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_Users", x => x.Id);
            //    });

            migrationBuilder.CreateTable(
                name: "WaitingResponOrders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NamaCustomer = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ItemService = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    JadwalService = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NoWA = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Alamat = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DeskripsiProblem = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TanggalOrder = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsTaken = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WaitingResponOrders", x => x.Id);
                });

        //    migrationBuilder.CreateTable(
        //        name: "Orders",
        //        columns: table => new
        //        {
        //            Id = table.Column<int>(type: "int", nullable: false)
        //                .Annotation("SqlServer:Identity", "1, 1"),
        //            Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
        //            CustomerId = table.Column<int>(type: "int", nullable: true),
        //            TechnicianId = table.Column<int>(type: "int", nullable: true),
        //            Status = table.Column<string>(type: "nvarchar(max)", nullable: false)
        //        },
        //        constraints: table =>
        //        {
        //            table.PrimaryKey("PK_Orders", x => x.Id);
        //            table.ForeignKey(
        //                name: "FK_Orders_Users_CustomerId",
        //                column: x => x.CustomerId,
        //                principalTable: "Users",
        //                principalColumn: "Id",
        //                onDelete: ReferentialAction.Restrict);
        //            table.ForeignKey(
        //                name: "FK_Orders_Users_TechnicianId",
        //                column: x => x.TechnicianId,
        //                principalTable: "Users",
        //                principalColumn: "Id",
        //                onDelete: ReferentialAction.Restrict);
        //        });

        //    migrationBuilder.CreateIndex(
        //        name: "IX_Orders_CustomerId",
        //        table: "Orders",
        //        column: "CustomerId");

        //    migrationBuilder.CreateIndex(
        //        name: "IX_Orders_TechnicianId",
        //        table: "Orders",
        //        column: "TechnicianId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ActiveOrders");

            //migrationBuilder.DropTable(
            //    name: "AdminNets");

            //migrationBuilder.DropTable(
            //    name: "Orders");

            //migrationBuilder.DropTable(
            //    name: "Technicians");

            migrationBuilder.DropTable(
                name: "WaitingResponOrders");

            //migrationBuilder.DropTable(
            //    name: "Users");
        }
    }
}
