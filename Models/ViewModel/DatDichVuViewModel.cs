using System;
using System.ComponentModel.DataAnnotations;

namespace DACN.Models.ViewModel
{
    public class DatDichVuViewModel
    {
        public int MaDichVu { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime NgaySuDung { get; set; } = DateTime.Today;

        [Required]
        [Display(Name = "Khung giờ")]
        public TimeSpan KhungGio { get; set; } // thêm dòng này

        [Required]
        [Range(1, 100)]
        public int SoLuong { get; set; } = 1;
        [Display(Name = "Ghi chú")]
        public string GhiChu { get; set; } = "";
    }

    public class XacNhanDichVuViewModel
    {
        public DACN.Models.KhachHang KhachHang { get; set; }
        public DACN.Models.DichVu DichVu { get; set; }
        public DateTime NgaySuDung { get; set; }
        [Display(Name = "Khung giờ")]
        public TimeSpan KhungGio { get; set; } // thêm dòng này
        public int SoLuong { get; set; }
        public decimal ThanhTien { get; set; }
        [Display(Name = "Ghi chú")]
        public string GhiChu { get; set; } = "";
    }
}