using System;
using System.Collections.Generic;

namespace DACN.Models
{
    public class CuocTroChuyen
    {
        public int Id { get; set; }

        // nếu customer đăng ký trong hệ thống, lưu Id; nếu guest thì null và lưu TenKhach
        public int? KhachHangId { get; set; }
        public string? TenKhachHang { get; set; }

        // Nếu nhân viên đã nhận cuộc chat thì ghi NhanVienId, null nếu chưa
        public int? NhanVienId { get; set; }
        public string? TenNhanVien { get; set; }

        public DateTime BatDauLuc { get; set; } = DateTime.UtcNow;
        public DateTime? KetThucLuc { get; set; }

        public virtual List<TinNhan> TinNhans { get; set; } = new();
    }
}
