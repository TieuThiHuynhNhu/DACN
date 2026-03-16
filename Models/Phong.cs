using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DACN.Models
{
    public class Phong
    {
        [Key]
        public int MaPhong { get; set; }

        public string? LoaiPhong { get; set; }
        public decimal GiaPhong { get; set; }
        public string? MoTa { get; set; }  // thêm mô tả, có thể để null

        public string TrangThai { get; set; } // ConTrong, DaDat, ...
        public string? HinhAnh { get; set; }

        [NotMapped]
        public decimal GiaGoc { get; set; }
        // 🔥 Thêm FOREIGN KEY
        public int? MaKhuyenMai { get; set; }  // nullable = có thể không có KM

        // 🔥 Điều chỉnh quan hệ (Navigation Property)
        public KhuyenMai? KhuyenMai { get; set; }

        // Nhiều-nhiều với KhuyenMai
        public ICollection<PhongKhuyenMai> PhongKhuyenMais { get; set; } = new List<PhongKhuyenMai>();
        // 🔹 Thuộc tính tạm thời để hiển thị khuyến mãi đang áp dụng
    [NotMapped]
    public KhuyenMai? KhuyenMaiHienTai { get; set; }

        [NotMapped]
        public double AverageStars { get; set; }

        [NotMapped]
        public int TotalReviews { get; set; }

    }

}
