using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DACN.Migrations
{
    /// <inheritdoc />
    public partial class UpdatePhieuHuHaiAndThongBao : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "GhiChuXuLy",
                table: "PhieuHuHai",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MaNguoiTao",
                table: "PhieuHuHai",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NguonPhieu",
                table: "PhieuHuHai",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GhiChuXuLy",
                table: "PhieuHuHai");

            migrationBuilder.DropColumn(
                name: "MaNguoiTao",
                table: "PhieuHuHai");

            migrationBuilder.DropColumn(
                name: "NguonPhieu",
                table: "PhieuHuHai");
        }
    }
}
