using System.ComponentModel.DataAnnotations;

namespace DACN.Models
{
    public class DatPhongViewModel
    {
        public int MaPhong { get; set; }
        public DateTime NgayNhan { get; set; }
        public DateTime NgayTra { get; set; }
        public decimal GiaPhong { get; set; }
        public decimal TongTien => (NgayTra - NgayNhan).Days * GiaPhong;
        // ⭐ THÊM THUỘC TÍNH NÀY ⭐
        [Required(ErrorMessage = "Vui lòng chọn hình thức thanh toán.")]
        public string HinhThucThanhToan { get; set; }

        public string? GhiChu { get; set; }
        // Trong DatPhong Model
        public int MaKhachHang { get; set; }
      
        public KhachHang? KhachHang { get; set; } // Navigation property

       
    }

}
