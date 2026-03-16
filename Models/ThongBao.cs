using System.ComponentModel.DataAnnotations;

namespace DACN.Models
{
    public class ThongBao
    {
        [Key]
        public int Id { get; set; }

        public string? NoiDung { get; set; }

        public DateTime ThoiGian { get; set; } = DateTime.Now;

        public bool DaDoc { get; set; } = false;
        public string? NguoiNhan { get; set; } // tên đăng nhập nhân viên nhận thông báo
    }
}
