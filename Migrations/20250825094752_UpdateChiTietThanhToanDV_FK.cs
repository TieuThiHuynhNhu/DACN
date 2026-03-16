using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DACN.Migrations
{
    /// <inheritdoc />
    public partial class UpdateChiTietThanhToanDV_FK : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ChiTietThanhToanDV_KhachHang_MaKhachHang",
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

            migrationBuilder.AlterColumn<int>(
                name: "MaKhachHang",
                table: "ChiTietThanhToanDV",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AddForeignKey(
                name: "FK_ChiTietThanhToanDV_KhachHang_MaKhachHang",
                table: "ChiTietThanhToanDV",
                column: "MaKhachHang",
                principalTable: "KhachHang",
                principalColumn: "MaKhachHang");
        }
    }
}
