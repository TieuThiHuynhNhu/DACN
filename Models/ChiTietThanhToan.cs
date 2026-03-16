using System.ComponentModel.DataAnnotations;

namespace DACN.Models
{
    public class ChiTietThanhToan
    {
        [Key]
        public int MaChiTiet { get; set; }

        public int MaThanhToan { get; set; }
        public int MaDichVu { get; set; }
        public int SoLuong { get; set; }
        public decimal DonGia { get; set; }
        public decimal ThanhTien { get; set; }
        public string HinhThucThanhToan { get; set; }
    }

}
