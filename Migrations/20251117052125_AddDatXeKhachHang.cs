using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DACN.Migrations
{
    /// <inheritdoc />
    public partial class AddDatXeKhachHang : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DatPhong_Phong_MaPhong",
                table: "DatPhong");

            migrationBuilder.DropTable(
                name: "GioHang");

            migrationBuilder.AlterColumn<int>(
                name: "MaPhong",
                table: "DatPhong",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AlterColumn<TimeSpan>(
                name: "KhungGio",
                table: "DatDichVu",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(TimeSpan),
                oldType: "TEXT");

            migrationBuilder.CreateTable(
                name: "DatXe",
                columns: table => new
                {
                    MaDatXe = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TenKhachHang = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    DiaDiemDi = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    DiaDiemDen = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    KhoangCach = table.Column<double>(type: "decimal(10,2)", nullable: false),
                    GiaTien = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    LoaiXe = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    NgayDat = table.Column<DateTime>(type: "TEXT", nullable: false),
                    GhiChu = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    MaKhachHang = table.Column<int>(type: "INTEGER", nullable: false),
                    KhachHangMaKhachHang = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DatXe", x => x.MaDatXe);
                    table.ForeignKey(
                        name: "FK_DatXe_KhachHang_KhachHangMaKhachHang",
                        column: x => x.KhachHangMaKhachHang,
                        principalTable: "KhachHang",
                        principalColumn: "MaKhachHang",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DatXe_KhachHangMaKhachHang",
                table: "DatXe",
                column: "KhachHangMaKhachHang");

            migrationBuilder.AddForeignKey(
                name: "FK_DatPhong_Phong_MaPhong",
                table: "DatPhong",
                column: "MaPhong",
                principalTable: "Phong",
                principalColumn: "MaPhong");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DatPhong_Phong_MaPhong",
                table: "DatPhong");

            migrationBuilder.DropTable(
                name: "DatXe");

            migrationBuilder.AlterColumn<int>(
                name: "MaPhong",
                table: "DatPhong",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true);

            migrationBuilder.AlterColumn<TimeSpan>(
                name: "KhungGio",
                table: "DatDichVu",
                type: "TEXT",
                nullable: false,
                defaultValue: new TimeSpan(0, 0, 0, 0, 0),
                oldClrType: typeof(TimeSpan),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "GioHang",
                columns: table => new
                {
                    MaGioHang = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    MaDichVu = table.Column<int>(type: "INTEGER", nullable: false),
                    DonGia = table.Column<decimal>(type: "TEXT", nullable: false),
                    GhiChu = table.Column<string>(type: "TEXT", nullable: false),
                    MaKhachHang = table.Column<int>(type: "INTEGER", nullable: false),
                    NgayThem = table.Column<DateTime>(type: "TEXT", nullable: true),
                    SoLuong = table.Column<int>(type: "INTEGER", nullable: false),
                    ThanhTien = table.Column<decimal>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GioHang", x => x.MaGioHang);
                    table.ForeignKey(
                        name: "FK_GioHang_DichVu_MaDichVu",
                        column: x => x.MaDichVu,
                        principalTable: "DichVu",
                        principalColumn: "MaDichVu",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GioHang_MaDichVu",
                table: "GioHang",
                column: "MaDichVu");

            migrationBuilder.AddForeignKey(
                name: "FK_DatPhong_Phong_MaPhong",
                table: "DatPhong",
                column: "MaPhong",
                principalTable: "Phong",
                principalColumn: "MaPhong",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
