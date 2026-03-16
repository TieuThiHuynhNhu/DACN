using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DACN.Migrations
{
    /// <inheritdoc />
    public partial class ThemDanhGiaPhong : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DanhGiaPhongs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    MaPhong = table.Column<int>(type: "INTEGER", nullable: false),
                    MaKhachHang = table.Column<int>(type: "INTEGER", nullable: false),
                    SoSao = table.Column<int>(type: "INTEGER", nullable: false),
                    NoiDung = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    NgayDanhGia = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DanhGiaPhongs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DanhGiaPhongs_KhachHang_MaKhachHang",
                        column: x => x.MaKhachHang,
                        principalTable: "KhachHang",
                        principalColumn: "MaKhachHang",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DanhGiaPhongs_Phong_MaPhong",
                        column: x => x.MaPhong,
                        principalTable: "Phong",
                        principalColumn: "MaPhong",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DanhGiaPhongs_MaKhachHang",
                table: "DanhGiaPhongs",
                column: "MaKhachHang");

            migrationBuilder.CreateIndex(
                name: "IX_DanhGiaPhongs_MaPhong",
                table: "DanhGiaPhongs",
                column: "MaPhong");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DanhGiaPhongs");
        }
    }
}
