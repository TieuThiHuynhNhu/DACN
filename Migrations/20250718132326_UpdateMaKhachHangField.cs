using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DACN.Migrations
{
    /// <inheritdoc />
    public partial class UpdateMaKhachHangField : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ChiTietThanhToanDV_KhachHang_MaKhachHang",
                table: "ChiTietThanhToanDV");

            migrationBuilder.DropIndex(
                name: "IX_ChiTietThanhToanDV_MaKhachHang",
                table: "ChiTietThanhToanDV");

            migrationBuilder.AlterColumn<int>(
                name: "MaKhachHang",
                table: "ChiTietThanhToanDV",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AddColumn<int>(
                name: "KhachHangMaKhachHang",
                table: "ChiTietThanhToanDV",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_ChiTietThanhToanDV_KhachHangMaKhachHang",
                table: "ChiTietThanhToanDV",
                column: "KhachHangMaKhachHang");

            migrationBuilder.AddForeignKey(
                name: "FK_ChiTietThanhToanDV_KhachHang_KhachHangMaKhachHang",
                table: "ChiTietThanhToanDV",
                column: "KhachHangMaKhachHang",
                principalTable: "KhachHang",
                principalColumn: "MaKhachHang",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ChiTietThanhToanDV_KhachHang_KhachHangMaKhachHang",
                table: "ChiTietThanhToanDV");

            migrationBuilder.DropIndex(
                name: "IX_ChiTietThanhToanDV_KhachHangMaKhachHang",
                table: "ChiTietThanhToanDV");

            migrationBuilder.DropColumn(
                name: "KhachHangMaKhachHang",
                table: "ChiTietThanhToanDV");

            migrationBuilder.AlterColumn<int>(
                name: "MaKhachHang",
                table: "ChiTietThanhToanDV",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true);

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
    }
}
