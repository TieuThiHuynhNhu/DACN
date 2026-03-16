using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace DACN.Models
{
    public class DanhGiaPhong
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int MaPhong { get; set; }

        [ForeignKey("MaPhong")]
        public Phong Phong { get; set; }

        [Required]
        public int MaKhachHang { get; set; }

        [ForeignKey("MaKhachHang")]
        public KhachHang KhachHang { get; set; }

        [Range(1, 5)]
        public int SoSao { get; set; }  // 1-5 sao

        [MaxLength(500)]
        public string? NoiDung { get; set; }

        public DateTime NgayDanhGia { get; set; } = DateTime.Now;
    }
}
