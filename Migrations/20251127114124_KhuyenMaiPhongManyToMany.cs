using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DACN.Migrations
{
    /// <inheritdoc />
    public partial class KhuyenMaiPhongManyToMany : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Phong_KhuyenMai_MaKhuyenMai",
                table: "Phong");

            migrationBuilder.DropIndex(
                name: "IX_Phong_MaKhuyenMai",
                table: "Phong");

            migrationBuilder.DropIndex(
                name: "IX_KhuyenMai_MaPhong",
                table: "KhuyenMai");

            migrationBuilder.CreateTable(
                name: "PhongKhuyenMai",
                columns: table => new
                {
                    MaPhong = table.Column<int>(type: "INTEGER", nullable: false),
                    MaKhuyenMai = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PhongKhuyenMai", x => new { x.MaPhong, x.MaKhuyenMai });
                    table.ForeignKey(
                        name: "FK_PhongKhuyenMai_KhuyenMai_MaKhuyenMai",
                        column: x => x.MaKhuyenMai,
                        principalTable: "KhuyenMai",
                        principalColumn: "MaKhuyenMai",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PhongKhuyenMai_Phong_MaPhong",
                        column: x => x.MaPhong,
                        principalTable: "Phong",
                        principalColumn: "MaPhong",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_KhuyenMai_MaPhong",
                table: "KhuyenMai",
                column: "MaPhong",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PhongKhuyenMai_MaKhuyenMai",
                table: "PhongKhuyenMai",
                column: "MaKhuyenMai");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PhongKhuyenMai");

            migrationBuilder.DropIndex(
                name: "IX_KhuyenMai_MaPhong",
                table: "KhuyenMai");

            migrationBuilder.CreateIndex(
                name: "IX_Phong_MaKhuyenMai",
                table: "Phong",
                column: "MaKhuyenMai");

            migrationBuilder.CreateIndex(
                name: "IX_KhuyenMai_MaPhong",
                table: "KhuyenMai",
                column: "MaPhong");

            migrationBuilder.AddForeignKey(
                name: "FK_Phong_KhuyenMai_MaKhuyenMai",
                table: "Phong",
                column: "MaKhuyenMai",
                principalTable: "KhuyenMai",
                principalColumn: "MaKhuyenMai",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
