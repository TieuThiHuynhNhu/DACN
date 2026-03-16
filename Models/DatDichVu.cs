using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DACN.Models
{
    public class DatDichVu
    {
        [Key]
        public int MaDatDichVu { get; set; }

        // FK tới KhachHang
        public int MaKhachHang { get; set; }
        [ForeignKey("MaKhachHang")]
        public KhachHang? KhachHang { get; set; }   // Navigation property

        // FK tới DichVu
        public int MaDichVu { get; set; }
        [ForeignKey("MaDichVu")]
        public DichVu? DichVu { get; set; }         // Navigation property

        public DateTime? NgaySuDung { get; set; }
        public TimeSpan? KhungGio { get; set; } // 08:00 - 09:00, 09:00 - 10:00,...
        [Required]
        public int SoLuong { get; set; } // số lượng dịch vụ

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal ThanhTien { get; set; } // SoLuong * DichVu.GiaDichVu
        public string TrangThai { get; set; }
        public string GhiChu { get; set; }

        // Quan hệ 1-n với ChiTietThanhToanDV
        public ICollection<ChiTietThanhToanDV>? ChiTietThanhToanDVs { get; set; }
    }
}
