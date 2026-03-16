using System.ComponentModel.DataAnnotations;

namespace DACN.Models
{
    public class NhanVien
    {
        [Key]
        public int MaNhanVien { get; set; }

        public string HoTen { get; set; }
        public string GioiTinh { get; set; }
        public string ViTri { get; set; }
        public string CaLamViec { get; set; }
        public string TaiKhoanDangNhap { get; set; }
        public string MatKhau { get; set; }
        public string? HinhAnhKhuonMat { get; set; }

        public ICollection<ChamCong> ChamCongs { get; set; }

        public string? SoDienThoai { get; set; }
        public string? Email { get; set; }
        public string? DiaChi { get; set; }
        public DateTime? NgaySinh { get; set; }
      
    }

}
