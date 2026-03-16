using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Http;

namespace DACN.Models
{
    public class SliderModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tiêu đề")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập mô tả")]
        public string MoTa { get; set; }

        public int? HienThi { get; set; } // 1: Hiện, 0: Ẩn

        public string? HinhAnh { get; set; } // Tên file hình ảnh

        [NotMapped]
        [Display(Name = "Ảnh đại diện")]
        public IFormFile? HinhAnhFile { get; set; } // File hình ảnh tải lên
    }
}
