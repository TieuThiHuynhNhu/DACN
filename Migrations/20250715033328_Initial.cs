using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DACN.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BaoCao",
                columns: table => new
                {
                    MaBaoCao = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    LoaiBaoCao = table.Column<string>(type: "TEXT", nullable: false),
                    NgayTao = table.Column<DateTime>(type: "TEXT", nullable: false),
                    PhamViThoiGian = table.Column<string>(type: "TEXT", nullable: false),
                    NguoiTao = table.Column<string>(type: "TEXT", nullable: false),
                    chi_tiet = table.Column<string>(type: "TEXT", nullable: true),
                    trang_thai = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BaoCao", x => x.MaBaoCao);
                });

            migrationBuilder.CreateTable(
                name: "ChamCong",
                columns: table => new
                {
                    MaChamCong = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    MaNhanVien = table.Column<int>(type: "INTEGER", nullable: false),
                    NgayGioVaoCa = table.Column<DateTime>(type: "TEXT", nullable: false),
                    NgayGioKetCa = table.Column<DateTime>(type: "TEXT", nullable: false),
                    AnhNhanDienKhuonMat = table.Column<string>(type: "TEXT", nullable: false),
                    GhiChu = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChamCong", x => x.MaChamCong);
                });

            migrationBuilder.CreateTable(
                name: "ChiTietThanhToan",
                columns: table => new
                {
                    MaChiTiet = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    MaThanhToan = table.Column<int>(type: "INTEGER", nullable: false),
                    MaDichVu = table.Column<int>(type: "INTEGER", nullable: false),
                    SoLuong = table.Column<int>(type: "INTEGER", nullable: false),
                    DonGia = table.Column<decimal>(type: "TEXT", nullable: false),
                    ThanhTien = table.Column<decimal>(type: "TEXT", nullable: false),
                    HinhThucThanhToan = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChiTietThanhToan", x => x.MaChiTiet);
                });

            migrationBuilder.CreateTable(
                name: "ChiTietThanhToanDV",
                columns: table => new
                {
                    MaChiTiet = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    MaDichVu = table.Column<string>(type: "TEXT", nullable: false),
                    
                    HinhThucThanhToan = table.Column<string>(type: "TEXT", nullable: false),
                    TongTien = table.Column<decimal>(type: "TEXT", nullable: false),
                    NgayThanhToan = table.Column<DateTime>(type: "TEXT", nullable: false),
                    GhiChu = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChiTietThanhToanDV", x => x.MaChiTiet);
                });

            migrationBuilder.CreateTable(
                name: "DatDichVu",
                columns: table => new
                {
                    MaDatDichVu = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    MaKhachHang = table.Column<int>(type: "INTEGER", nullable: false),
                    MaDichVu = table.Column<int>(type: "INTEGER", nullable: false),
                    NgaySuDung = table.Column<DateTime>(type: "TEXT", nullable: false),
                    TrangThai = table.Column<string>(type: "TEXT", nullable: false),
                    GhiChu = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DatDichVu", x => x.MaDatDichVu);
                });

            migrationBuilder.CreateTable(
                name: "DatPhong",
                columns: table => new
                {
                    MaDatPhong = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    MaKhachHang = table.Column<int>(type: "INTEGER", nullable: false),
                    MaPhong = table.Column<int>(type: "INTEGER", nullable: false),
                    NgayDat = table.Column<DateTime>(type: "TEXT", nullable: false),
                    NgayNhanPhong = table.Column<DateTime>(type: "TEXT", nullable: false),
                    NgayTraPhong = table.Column<DateTime>(type: "TEXT", nullable: false),
                    TrangThai = table.Column<string>(type: "TEXT", nullable: false),
                    HinhThucThanhToan = table.Column<string>(type: "TEXT", nullable: false),
                    TongTien = table.Column<decimal>(type: "TEXT", nullable: false),
                    GhiChu = table.Column<string>(type: "TEXT", nullable: false),
                    NgayNhan = table.Column<DateTime>(type: "TEXT", nullable: true),
                    NgayTra = table.Column<DateTime>(type: "TEXT", nullable: true),
                    MaQR = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DatPhong", x => x.MaDatPhong);
                });

            migrationBuilder.CreateTable(
                name: "DichVu",
                columns: table => new
                {
                    MaDichVu = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TenDichVu = table.Column<string>(type: "TEXT", nullable: false),
                    GiaDichVu = table.Column<decimal>(type: "TEXT", nullable: false),
                    MoTa = table.Column<string>(type: "TEXT", nullable: false),
                    TrangThai = table.Column<string>(type: "TEXT", nullable: false),
                    MaDichVuCha = table.Column<int>(type: "INTEGER", nullable: true),
                    HinhAnh = table.Column<string>(type: "TEXT", nullable: false),
                    MucLuc = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DichVu", x => x.MaDichVu);
                });

            migrationBuilder.CreateTable(
                name: "GioHang",
                columns: table => new
                {
                    MaGioHang = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    MaKhachHang = table.Column<int>(type: "INTEGER", nullable: false),
                    MaDichVu = table.Column<int>(type: "INTEGER", nullable: false),
                    SoLuong = table.Column<int>(type: "INTEGER", nullable: false),
                    DonGia = table.Column<decimal>(type: "TEXT", nullable: false),
                    ThanhTien = table.Column<decimal>(type: "TEXT", nullable: false),
                    GhiChu = table.Column<string>(type: "TEXT", nullable: false),
                    NgayThem = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GioHang", x => x.MaGioHang);
                });

            migrationBuilder.CreateTable(
                name: "KhachHang",
                columns: table => new
                {
                    MaKhachHang = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    HoTen = table.Column<string>(type: "TEXT", nullable: false),
                    DiaChi = table.Column<string>(type: "TEXT", nullable: false),
                    SoDienThoai = table.Column<string>(type: "TEXT", nullable: false),
                    Email = table.Column<string>(type: "TEXT", nullable: false),
                    CCCD = table.Column<string>(type: "TEXT", nullable: false),
                    TenDangNhap = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KhachHang", x => x.MaKhachHang);
                });

            migrationBuilder.CreateTable(
                name: "NhanVien",
                columns: table => new
                {
                    MaNhanVien = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    HoTen = table.Column<string>(type: "TEXT", nullable: false),
                    GioiTinh = table.Column<string>(type: "TEXT", nullable: false),
                    ViTri = table.Column<string>(type: "TEXT", nullable: false),
                    CaLamViec = table.Column<string>(type: "TEXT", nullable: false),
                    TaiKhoanDangNhap = table.Column<string>(type: "TEXT", nullable: false),
                    MatKhau = table.Column<string>(type: "TEXT", nullable: false),
                    HinhAnhKhuonMat = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NhanVien", x => x.MaNhanVien);
                });

            migrationBuilder.CreateTable(
                name: "Phong",
                columns: table => new
                {
                    MaPhong = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    LoaiPhong = table.Column<string>(type: "TEXT", nullable: false),
                    GiaPhong = table.Column<decimal>(type: "TEXT", nullable: false),
                    TrangThai = table.Column<string>(type: "TEXT", nullable: false),
                    HinhAnh = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Phong", x => x.MaPhong);
                });

            migrationBuilder.CreateTable(
                name: "TaiKhoan",
                columns: table => new
                {
                    TenDangNhap = table.Column<string>(type: "TEXT", nullable: false),
                    MatKhau = table.Column<string>(type: "TEXT", nullable: false),
                    QuyenTruyCap = table.Column<string>(type: "TEXT", nullable: false),
                    BaoMat2FA = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaiKhoan", x => x.TenDangNhap);
                });

            migrationBuilder.CreateTable(
                name: "ThanhToan",
                columns: table => new
                {
                    MaThanhToan = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    MaKhachHang = table.Column<int>(type: "INTEGER", nullable: false),
                    MaDatPhong = table.Column<int>(type: "INTEGER", nullable: false),
                    NgayThanhToan = table.Column<DateTime>(type: "TEXT", nullable: false),
                    SoTien = table.Column<decimal>(type: "TEXT", nullable: false),
                    HinhThucThanhToan = table.Column<string>(type: "TEXT", nullable: false),
                    TrangThai = table.Column<string>(type: "TEXT", nullable: false),
                    TongTien = table.Column<decimal>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ThanhToan", x => x.MaThanhToan);
                });

            migrationBuilder.CreateTable(
                name: "PhieuHuHai",
                columns: table => new
                {
                    MaPhieu = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    MaPhong = table.Column<int>(type: "INTEGER", nullable: false),
                    TenThietBi = table.Column<string>(type: "TEXT", nullable: false),
                    MucDoHuHai = table.Column<string>(type: "TEXT", nullable: false),
                    ThoiGianGhiNhan = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ChiPhiBoiThuong = table.Column<decimal>(type: "TEXT", nullable: false),
                    TrangThai = table.Column<string>(type: "TEXT", nullable: false),
                    NguyenNhan = table.Column<string>(type: "TEXT", nullable: false),
                    PhongMaPhong = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PhieuHuHai", x => x.MaPhieu);
                    table.ForeignKey(
                        name: "FK_PhieuHuHai_Phong_PhongMaPhong",
                        column: x => x.PhongMaPhong,
                        principalTable: "Phong",
                        principalColumn: "MaPhong");
                });

            migrationBuilder.CreateIndex(
                name: "IX_PhieuHuHai_PhongMaPhong",
                table: "PhieuHuHai",
                column: "PhongMaPhong");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BaoCao");

            migrationBuilder.DropTable(
                name: "ChamCong");

            migrationBuilder.DropTable(
                name: "ChiTietThanhToan");

            migrationBuilder.DropTable(
                name: "ChiTietThanhToanDV");

            migrationBuilder.DropTable(
                name: "DatDichVu");

            migrationBuilder.DropTable(
                name: "DatPhong");

            migrationBuilder.DropTable(
                name: "DichVu");

            migrationBuilder.DropTable(
                name: "GioHang");

            migrationBuilder.DropTable(
                name: "KhachHang");

            migrationBuilder.DropTable(
                name: "NhanVien");

            migrationBuilder.DropTable(
                name: "PhieuHuHai");

            migrationBuilder.DropTable(
                name: "TaiKhoan");

            migrationBuilder.DropTable(
                name: "ThanhToan");

            migrationBuilder.DropTable(
                name: "Phong");
        }
    }
}
