using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DACN.Migrations
{
    /// <inheritdoc />
    public partial class UpdateChamCong : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
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
        }
    }
}
