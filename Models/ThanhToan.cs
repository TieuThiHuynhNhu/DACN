using System.ComponentModel.DataAnnotations;

namespace DACN.Models
{
    public class ThanhToan
    {
        [Key]
        public int MaThanhToan { get; set; }

        public int MaKhachHang { get; set; }
        public int MaDatPhong { get; set; }
        public DateTime NgayThanhToan { get; set; }
        public decimal SoTien { get; set; }
        public string HinhThucThanhToan { get; set; } // Tiền mặt, Thẻ tín dụng,...
        public string TrangThai { get; set; }
        public decimal TongTien { get; set; }
    }

}
