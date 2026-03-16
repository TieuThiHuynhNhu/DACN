using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DACN.Migrations
{
    /// <inheritdoc />
    public partial class AddHinhAnhToKhachHang : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "trang_thai",
                table: "BaoCao",
                newName: "TrangThai");

            migrationBuilder.RenameColumn(
                name: "chi_tiet",
                table: "BaoCao",
                newName: "ChiTiet");

            migrationBuilder.AddColumn<string>(
                name: "HinhAnh",
                table: "KhachHang",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "PhamViThoiGian",
                table: "BaoCao",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<string>(
                name: "NguoiTao",
                table: "BaoCao",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HinhAnh",
                table: "KhachHang");

            migrationBuilder.RenameColumn(
                name: "TrangThai",
                table: "BaoCao",
                newName: "trang_thai");

            migrationBuilder.RenameColumn(
                name: "ChiTiet",
                table: "BaoCao",
                newName: "chi_tiet");

            migrationBuilder.AlterColumn<string>(
                name: "PhamViThoiGian",
                table: "BaoCao",
                type: "TEXT",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "NguoiTao",
                table: "BaoCao",
                type: "TEXT",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);
        }
    }
}
