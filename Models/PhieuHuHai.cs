using System.ComponentModel.DataAnnotations;

namespace DACN.Models
{
    public class PhieuHuHai
    {
        [Key]
        public int MaPhieu { get; set; }

        public int? MaPhong { get; set; }
        public string? TenThietBi { get; set; }
        public string? MucDoHuHai { get; set; }
        public DateTime ThoiGianGhiNhan { get; set; } = DateTime.Now;
        public decimal ChiPhiBoiThuong { get; set; }
        public string? TrangThai { get; set; }
        public string? NguyenNhan { get; set; }

        public string? AnhMinhChung { get; set; } // đường dẫn ảnh upload

        // Ai lập phiếu
        public string? NguonPhieu { get; set; } // "KhachHang" hoặc "NhanVien"
        public int? MaNguoiTao { get; set; } // lưu mã KH hoặc NV nếu cần
        public string? GhiChuXuLy { get; set; }
        // Khóa ngoại (nếu cần navigation property)
        public Phong? Phong { get; set; }
    }
}
