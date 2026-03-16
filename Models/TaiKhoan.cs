using System.ComponentModel.DataAnnotations;

namespace DACN.Models
{
    public class TaiKhoan
    {
        [Key]
        public string TenDangNhap { get; set; }
       
        public string MatKhau { get; set; }
        
        public string QuyenTruyCap { get; set; } // Admin, NhanVien, KhachHang
       
        public bool BaoMat2FA { get; set; }
        public bool BiChan { get; set; }  // thêm để quản lý khóa/mở tài khoản
    }

}
