using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DACN.Models
{
    public class DatXe
    {
        [Key] // Khai báo khóa chính rõ ràng
        public int MaDatXe { get; set; }

        [Required]
        [StringLength(100)]
        public string TenKhachHang { get; set; }  // Lấy từ session nếu login

        [Required]
        [StringLength(200)]
        public string DiaDiemDi { get; set; }

        [Required]
        [StringLength(200)]
        public string DiaDiemDen { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal KhoangCach { get; set; }    // km

        [Column(TypeName = "decimal(18,2)")]
        public decimal GiaTien { get; set; }

        [StringLength(50)]
        public string LoaiXe { get; set; }        // 4 chỗ, 7 chỗ, xe điện...

        public DateTime NgayDat { get; set; }

        [StringLength(500)]
        public string? GhiChu { get; set; }

        // Foreign key
        public int? MaKhachHang { get; set; }

        // Navigation property
        [ForeignKey("MaKhachHang")]
        public KhachHang? KhachHang { get; set; }
    }
}
