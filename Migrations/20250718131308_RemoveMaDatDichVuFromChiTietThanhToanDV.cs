using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DACN.Migrations
{
    /// <inheritdoc />
    public partial class RemoveMaDatDichVuFromChiTietThanhToanDV : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "MaDatDichVu",
                table: "ChiTietThanhToanDV",
                newName: "MaKhachHang");

            migrationBuilder.CreateIndex(
                name: "IX_ChiTietThanhToanDV_MaKhachHang",
                table: "ChiTietThanhToanDV",
                column: "MaKhachHang");

            migrationBuilder.AddForeignKey(
                name: "FK_ChiTietThanhToanDV_KhachHang_MaKhachHang",
                table: "ChiTietThanhToanDV",
                column: "MaKhachHang",
                principalTable: "KhachHang",
                principalColumn: "MaKhachHang",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ChiTietThanhToanDV_KhachHang_MaKhachHang",
                table: "ChiTietThanhToanDV");

            migrationBuilder.DropIndex(
                name: "IX_ChiTietThanhToanDV_MaKhachHang",
                table: "ChiTietThanhToanDV");

            migrationBuilder.RenameColumn(
                name: "MaKhachHang",
                table: "ChiTietThanhToanDV",
                newName: "MaDatDichVu");
        }
    }
}
