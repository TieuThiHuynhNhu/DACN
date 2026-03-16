using System;
using System.ComponentModel.DataAnnotations;

namespace DACN.Models
{
    public class BaoCao
    {
        [Key]
        public int MaBaoCao { get; set; }

        [Required]
        [Display(Name = "Loại báo cáo")]
        public string LoaiBaoCao { get; set; } = string.Empty;

        public DateTime NgayTao { get; set; } = DateTime.Now;

        [Display(Name = "Phạm vi thời gian")]
        public string? PhamViThoiGian { get; set; }

        [Display(Name = "Người tạo")]
        public string? NguoiTao { get; set; }

        [Display(Name = "Chi tiết")]
        public string? ChiTiet { get; set; }

        [Display(Name = "Trạng thái")]
        public string? TrangThai { get; set; } // Chưa xử lý, Đang xử lý, Đã xử lý
    }
}
