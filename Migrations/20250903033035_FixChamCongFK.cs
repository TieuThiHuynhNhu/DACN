using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DACN.Migrations
{
    /// <inheritdoc />
    public partial class FixChamCongFK : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ChamCong_NhanVien_NhanVienMaNhanVien",
                table: "ChamCong");

            migrationBuilder.DropIndex(
                name: "IX_ChamCong_NhanVienMaNhanVien",
                table: "ChamCong");

            migrationBuilder.DropColumn(
                name: "NhanVienMaNhanVien",
                table: "ChamCong");

            migrationBuilder.AlterColumn<string>(
                name: "GhiChu",
                table: "ChamCong",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<string>(
                name: "AnhNhanDienKhuonMat",
                table: "ChamCong",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.CreateIndex(
                name: "IX_ChamCong_MaNhanVien",
                table: "ChamCong",
                column: "MaNhanVien");

            migrationBuilder.AddForeignKey(
                name: "FK_ChamCong_NhanVien_MaNhanVien",
                table: "ChamCong",
                column: "MaNhanVien",
                principalTable: "NhanVien",
                principalColumn: "MaNhanVien",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ChamCong_NhanVien_MaNhanVien",
                table: "ChamCong");

            migrationBuilder.DropIndex(
                name: "IX_ChamCong_MaNhanVien",
                table: "ChamCong");

            migrationBuilder.AlterColumn<string>(
                name: "GhiChu",
                table: "ChamCong",
                type: "TEXT",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "AnhNhanDienKhuonMat",
                table: "ChamCong",
                type: "TEXT",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "NhanVienMaNhanVien",
                table: "ChamCong",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_ChamCong_NhanVienMaNhanVien",
                table: "ChamCong",
                column: "NhanVienMaNhanVien");

            migrationBuilder.AddForeignKey(
                name: "FK_ChamCong_NhanVien_NhanVienMaNhanVien",
                table: "ChamCong",
                column: "NhanVienMaNhanVien",
                principalTable: "NhanVien",
                principalColumn: "MaNhanVien",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
