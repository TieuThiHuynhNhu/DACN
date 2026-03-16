using DACN.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
namespace DACN.Models
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<KhachHang> KhachHang { get; set; }
        public DbSet<SliderModel> Sliders { get; set; }
        public DbSet<Phong> Phong { get; set; }
        public DbSet<DatPhong> DatPhong { get; set; }
        public DbSet<ThanhToan> ThanhToan { get; set; }
        public DbSet<DichVu> DichVu { get; set; }
        public DbSet<DatDichVu> DatDichVu { get; set; }
        public DbSet<NhanVien> NhanVien { get; set; }
        public DbSet<ChamCong> ChamCong { get; set; }
        public DbSet<BaoCao> BaoCao { get; set; }
        public DbSet<TaiKhoan> TaiKhoan { get; set; }
        public DbSet<DatXe> DatXe { get; set; }
        public DbSet<ChiTietThanhToan> ChiTietThanhToan { get; set; }
        public DbSet<ChiTietThanhToanDV> ChiTietThanhToanDV { get; set; }
        public DbSet<PhieuHuHai> PhieuHuHai { get; set; }
        public DbSet<KhuyenMai> KhuyenMai { get; set; }
        public DbSet<ThongBao> ThongBao { get; set; }

        public DbSet<DACN.Models.CuocTroChuyen> CuocTroChuyens { get; set; }
        public DbSet<DACN.Models.TinNhan> TinNhans { get; set; }
        public DbSet<DanhGiaPhong> DanhGiaPhongs { get; set; }

        public DbSet<DonThanhToanDichVu> DonThanhToanDichVu { get; set; }

        public DbSet<PhongKhuyenMai> PhongKhuyenMai { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<TaiKhoan>()
                .HasKey(t => t.TenDangNhap);

            modelBuilder.Entity<TaiKhoan>()
                .Property(t => t.BaoMat2FA)
                .HasDefaultValue(false);

            modelBuilder.Entity<PhongKhuyenMai>()
       .HasKey(pk => new { pk.MaPhong, pk.MaKhuyenMai });

            modelBuilder.Entity<PhongKhuyenMai>()
                .HasOne(pk => pk.Phong)
                .WithMany(p => p.PhongKhuyenMais)
                .HasForeignKey(pk => pk.MaPhong);

            modelBuilder.Entity<PhongKhuyenMai>()
                .HasOne(pk => pk.KhuyenMai)
                .WithMany(k => k.PhongKhuyenMais)
                .HasForeignKey(pk => pk.MaKhuyenMai);
            // ⭐ THÊM CẤU HÌNH CHO PHIEU HU HAI VÀ PHÒNG ⭐
            modelBuilder.Entity<PhieuHuHai>()
                .HasOne(phh => phh.Phong)         // Phiếu hư hại có MỘT Phòng
                .WithMany()                       // Phòng có NHIỀU Phiếu hư hại (không cần navigation property trong Phong)
                .HasForeignKey(phh => phh.MaPhong) // Khóa ngoại là MaPhong
                .IsRequired(false);               // Cho phép MaPhong null (vì bạn khai báo là int?)
        }

    }

}
