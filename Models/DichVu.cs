using System.ComponentModel.DataAnnotations;

namespace DACN.Models
{
    public class DichVu
    {
        [Key]
        public int MaDichVu { get; set; }

        public string TenDichVu { get; set; }
        public decimal? GiaDichVu { get; set; }
        public string? MoTa { get; set; }
        public string TrangThai { get; set; }
        
        public string? HinhAnh { get; set; }
        public string? MucLuc { get; set; }
    }

}
