using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DACN.Migrations
{
    /// <inheritdoc />
    public partial class Add_KhuyenMai_Into_Phong : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MaKhuyenMai",
                table: "Phong",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Phong_MaKhuyenMai",
                table: "Phong",
                column: "MaKhuyenMai");

            migrationBuilder.AddForeignKey(
                name: "FK_Phong_KhuyenMai_MaKhuyenMai",
                table: "Phong",
                column: "MaKhuyenMai",
                principalTable: "KhuyenMai",
                principalColumn: "MaKhuyenMai",
                onDelete: ReferentialAction.SetNull);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Phong_KhuyenMai_MaKhuyenMai",
                table: "Phong");

            migrationBuilder.DropIndex(
                name: "IX_Phong_MaKhuyenMai",
                table: "Phong");

            migrationBuilder.DropColumn(
                name: "MaKhuyenMai",
                table: "Phong");
        }

    }
}
