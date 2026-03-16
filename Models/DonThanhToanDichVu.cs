
using System.ComponentModel.DataAnnotations;
namespace DACN.Models
{
    public class DonThanhToanDichVu
    {
        [Key]
        public int MaDonDV { get; set; }

        public int MaKhachHang { get; set; }

        public decimal TongTien { get; set; }

        public string TrangThai { get; set; } // Pending / Success / Fail

        public DateTime NgayTao { get; set; } = DateTime.Now;

        public string? GhiChu { get; set; }
        public ICollection<ChiTietThanhToanDV>? ChiTietThanhToanDVs { get; set; }
    }
}
