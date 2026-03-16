using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DACN.Migrations
{
    /// <inheritdoc />
    public partial class Add_MaDatDichVu_To_ChiTietThanhToanDV : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_DatPhong_MaKhachHang",
                table: "DatPhong",
                column: "MaKhachHang");

            migrationBuilder.CreateIndex(
                name: "IX_DatPhong_MaPhong",
                table: "DatPhong",
                column: "MaPhong");

            migrationBuilder.AddForeignKey(
                name: "FK_DatPhong_KhachHang_MaKhachHang",
                table: "DatPhong",
                column: "MaKhachHang",
                principalTable: "KhachHang",
                principalColumn: "MaKhachHang",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DatPhong_Phong_MaPhong",
                table: "DatPhong",
                column: "MaPhong",
                principalTable: "Phong",
                principalColumn: "MaPhong",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DatPhong_KhachHang_MaKhachHang",
                table: "DatPhong");

            migrationBuilder.DropForeignKey(
                name: "FK_DatPhong_Phong_MaPhong",
                table: "DatPhong");

            migrationBuilder.DropIndex(
                name: "IX_DatPhong_MaKhachHang",
                table: "DatPhong");

            migrationBuilder.DropIndex(
                name: "IX_DatPhong_MaPhong",
                table: "DatPhong");
        }
    }
}
