using System.ComponentModel.DataAnnotations;

namespace DACN.Models
{
    public class KhachHang
    {
        [Key]
        public int MaKhachHang { get; set; }
        [Required]
        public string HoTen { get; set; }
        [Required]
        public string SoDienThoai { get; set; }
        [Required , EmailAddress]
        public string Email { get; set; }
        [Required]
        public string TenDangNhap { get; set; }
      
        public string MatKhau { get; set; }
    
        public string? HinhAnh { get; set; }
      
        public ICollection<DatXe>? DatXes { get; set; }
    }

}
