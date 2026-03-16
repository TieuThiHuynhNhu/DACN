using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DACN.Models
{
    public class ChiTietThanhToanDV
    {
        [Key]
        public int MaChiTiet { get; set; }

        public int MaDatDichVu { get; set; }
        [ForeignKey("MaDatDichVu")]
        public DatDichVu? DatDichVu { get; set; }   // Navigation property

        public int MaDichVu { get; set; }   // 🔥 thêm dòng này
        [ForeignKey("MaDichVu")]
        public DichVu? DichVu { get; set; } // Navigation property
        public string HinhThucThanhToan { get; set; }
        public decimal? TongTien { get; set; }
        public DateTime? NgayThanhToan { get; set; }
        public string GhiChu { get; set; }

        public int MaDonDV { get; set; }
        [ForeignKey("MaDonDV")]
        public DonThanhToanDichVu? DonThanhToanDichVu { get; set; }

    }
}
