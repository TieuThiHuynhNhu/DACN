using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DACN.Models
{
    public class ChamCong
    {
        [Key]
        public int MaChamCong { get; set; }

        [Required]
        public int MaNhanVien { get; set; }

        [ForeignKey("MaNhanVien")]
        public NhanVien NhanVien { get; set; }

        public DateTime NgayGioVaoCa { get; set; }
        public DateTime? NgayGioKetCa { get; set; }
        public string? AnhNhanDienKhuonMat { get; set; }
        public string? GhiChu { get; set; }
    }
}
