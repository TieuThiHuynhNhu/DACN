using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DACN.Models
{
    public class KhuyenMai
    {
        [Key]
        public int MaKhuyenMai { get; set; }

        [Required]
        public string TenKhuyenMai { get; set; }

        [Range(0, 100)]
        public double PhanTramGiam { get; set; } // % giảm giá

        [DataType(DataType.Date)]
        public DateTime NgayBatDau { get; set; }

        [DataType(DataType.Date)]
        public DateTime NgayKetThuc { get; set; }

        public string MoTa { get; set; }

        // Khuyến mãi áp dụng cho phòng cụ thể
        
        public int? MaPhong { get; set; }

        [ForeignKey("MaPhong")]
        public virtual Phong? Phong { get; set; }

        // ✅ Thêm hình khuyến mãi
        public string? HinhAnh { get; set; }


        // Danh sách mã phòng được chọn
        [NotMapped]
        public ICollection<Phong>? Phongs { get; set; } = new List<Phong>();

        // Nhiều-nhiều với Phong
        public ICollection<PhongKhuyenMai> PhongKhuyenMais { get; set; } = new List<PhongKhuyenMai>();


    }
}
