using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DACN.Migrations
{
    /// <inheritdoc />
    public partial class ChatInit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CuocTroChuyens",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    KhachHangId = table.Column<int>(type: "INTEGER", nullable: true),
                    TenKhachHang = table.Column<string>(type: "TEXT", nullable: true),
                    NhanVienId = table.Column<int>(type: "INTEGER", nullable: true),
                    TenNhanVien = table.Column<string>(type: "TEXT", nullable: true),
                    BatDauLuc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    KetThucLuc = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CuocTroChuyens", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TinNhans",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CuocTroChuyenId = table.Column<int>(type: "INTEGER", nullable: false),
                    GuiBoiRole = table.Column<string>(type: "TEXT", nullable: false),
                    GuiBoiName = table.Column<string>(type: "TEXT", nullable: false),
                    NoiDung = table.Column<string>(type: "TEXT", nullable: false),
                    ThoiGian = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TinNhans", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TinNhans_CuocTroChuyens_CuocTroChuyenId",
                        column: x => x.CuocTroChuyenId,
                        principalTable: "CuocTroChuyens",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TinNhans_CuocTroChuyenId",
                table: "TinNhans",
                column: "CuocTroChuyenId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TinNhans");

            migrationBuilder.DropTable(
                name: "CuocTroChuyens");
        }
    }
}
