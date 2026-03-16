using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DACN.Migrations
{
    /// <inheritdoc />
    public partial class AddMaDonDVToChiTietThanhToanDV : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "GiaDichVu",
                table: "DichVu",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<DateTime>(
                name: "NgaySuDung",
                table: "DatDichVu",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<decimal>(
                name: "TongTien",
                table: "ChiTietThanhToanDV",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<DateTime>(
                name: "NgayThanhToan",
                table: "ChiTietThanhToanDV",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "TEXT");

            migrationBuilder.AddColumn<int>(
                name: "MaDonDV",
                table: "ChiTietThanhToanDV",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_ChiTietThanhToanDV_MaDonDV",
                table: "ChiTietThanhToanDV",
                column: "MaDonDV");

            migrationBuilder.AddForeignKey(
                name: "FK_ChiTietThanhToanDV_DonThanhToanDichVu_MaDonDV",
                table: "ChiTietThanhToanDV",
                column: "MaDonDV",
                principalTable: "DonThanhToanDichVu",
                principalColumn: "MaDonDV",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ChiTietThanhToanDV_DonThanhToanDichVu_MaDonDV",
                table: "ChiTietThanhToanDV");

            migrationBuilder.DropIndex(
                name: "IX_ChiTietThanhToanDV_MaDonDV",
                table: "ChiTietThanhToanDV");

            migrationBuilder.DropColumn(
                name: "MaDonDV",
                table: "ChiTietThanhToanDV");

            migrationBuilder.AlterColumn<decimal>(
                name: "GiaDichVu",
                table: "DichVu",
                type: "TEXT",
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "NgaySuDung",
                table: "DatDichVu",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "TongTien",
                table: "ChiTietThanhToanDV",
                type: "TEXT",
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "NgayThanhToan",
                table: "ChiTietThanhToanDV",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldNullable: true);
        }
    }
}
