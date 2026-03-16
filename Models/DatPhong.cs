using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DACN.Models
{
    public class DatPhong
    {
        [Key]
        public int MaDatPhong { get; set; }

        public int MaKhachHang { get; set; }
        public int? MaPhong { get; set; }
        public DateTime NgayDat { get; set; }
        public DateTime NgayNhanPhong { get; set; }
        public DateTime NgayTraPhong { get; set; }
        public string TrangThai { get; set; } // ChoXacNhan, DaXacNhan,...
        public string HinhThucThanhToan { get; set; }
        public decimal TongTien { get; set; }
        public string GhiChu { get; set; }
        public DateTime? NgayNhan { get; set; }
        public DateTime? NgayTra { get; set; }
        [NotMapped]
        public string? MaQR { get; set; }
        // Thêm trường mới để lưu thời hạn hủy phòng
        public DateTime? HanHuyPhong { get; set; } // Thời điểm cuối cùng có thể hủy miễn phí

        [NotMapped]
        public decimal GiaPhong { get; set; }

        // Tổng tiền tính theo số ngày x Giá phòng
        [NotMapped]
        public decimal GrandTotal
        {
            get
            {
                if (NgayNhan.HasValue && NgayTra.HasValue && GiaPhong > 0)
                {
                    var soNgay = (NgayTra.Value - NgayNhan.Value).Days;
                    return soNgay > 0 ? soNgay * GiaPhong : GiaPhong;
                }
                return 0;
            }
        }
        [ForeignKey("MaPhong")]
        public Phong Phong { get; set; } // navigation property

        [ForeignKey("MaKhachHang")]
        public KhachHang KhachHang { get; set; }
    }

}
