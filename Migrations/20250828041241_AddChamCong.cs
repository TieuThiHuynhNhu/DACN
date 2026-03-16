using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DACN.Migrations
{
    /// <inheritdoc />
    public partial class AddChamCong : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ChiTietThanhToanDV_KhachHang_MaKhachHang",
                table: "ChiTietThanhToanDV");

            migrationBuilder.RenameColumn(
                name: "MaKhachHang",
                table: "ChiTietThanhToanDV",
                newName: "MaDatDichVu");

            migrationBuilder.RenameIndex(
                name: "IX_ChiTietThanhToanDV_MaKhachHang",
                table: "ChiTietThanhToanDV",
                newName: "IX_ChiTietThanhToanDV_MaDatDichVu");

            migrationBuilder.AlterColumn<int>(
                name: "MaDichVu",
                table: "ChiTietThanhToanDV",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.CreateIndex(
                name: "IX_DatDichVu_MaDichVu",
                table: "DatDichVu",
                column: "MaDichVu");

            migrationBuilder.CreateIndex(
                name: "IX_DatDichVu_MaKhachHang",
                table: "DatDichVu",
                column: "MaKhachHang");

            migrationBuilder.CreateIndex(
                name: "IX_ChiTietThanhToanDV_MaDichVu",
                table: "ChiTietThanhToanDV",
                column: "MaDichVu");

            migrationBuilder.AddForeignKey(
                name: "FK_ChiTietThanhToanDV_DatDichVu_MaDatDichVu",
                table: "ChiTietThanhToanDV",
                column: "MaDatDichVu",
                principalTable: "DatDichVu",
                principalColumn: "MaDatDichVu",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ChiTietThanhToanDV_DichVu_MaDichVu",
                table: "ChiTietThanhToanDV",
                column: "MaDichVu",
                principalTable: "DichVu",
                principalColumn: "MaDichVu",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DatDichVu_DichVu_MaDichVu",
                table: "DatDichVu",
                column: "MaDichVu",
                principalTable: "DichVu",
                principalColumn: "MaDichVu",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DatDichVu_KhachHang_MaKhachHang",
                table: "DatDichVu",
                column: "MaKhachHang",
                principalTable: "KhachHang",
                principalColumn: "MaKhachHang",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ChiTietThanhToanDV_DatDichVu_MaDatDichVu",
                table: "ChiTietThanhToanDV");

            migrationBuilder.DropForeignKey(
                name: "FK_ChiTietThanhToanDV_DichVu_MaDichVu",
                table: "ChiTietThanhToanDV");

            migrationBuilder.DropForeignKey(
                name: "FK_DatDichVu_DichVu_MaDichVu",
                table: "DatDichVu");

            migrationBuilder.DropForeignKey(
                name: "FK_DatDichVu_KhachHang_MaKhachHang",
                table: "DatDichVu");

            migrationBuilder.DropIndex(
                name: "IX_DatDichVu_MaDichVu",
                table: "DatDichVu");

            migrationBuilder.DropIndex(
                name: "IX_DatDichVu_MaKhachHang",
                table: "DatDichVu");

            migrationBuilder.DropIndex(
                name: "IX_ChiTietThanhToanDV_MaDichVu",
                table: "ChiTietThanhToanDV");

            migrationBuilder.RenameColumn(
                name: "MaDatDichVu",
                table: "ChiTietThanhToanDV",
                newName: "MaKhachHang");

            migrationBuilder.RenameIndex(
                name: "IX_ChiTietThanhToanDV_MaDatDichVu",
                table: "ChiTietThanhToanDV",
                newName: "IX_ChiTietThanhToanDV_MaKhachHang");

            migrationBuilder.AlterColumn<string>(
                name: "MaDichVu",
                table: "ChiTietThanhToanDV",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AddForeignKey(
                name: "FK_ChiTietThanhToanDV_KhachHang_MaKhachHang",
                table: "ChiTietThanhToanDV",
                column: "MaKhachHang",
                principalTable: "KhachHang",
                principalColumn: "MaKhachHang",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
