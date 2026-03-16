using DACN.Models;

namespace DACN.Models.ViewModel
{
    public class LichSuDatViewModel
    {
        public List<DatPhong> LichSuDatPhong { get; set; }
        public DateTime? NgayTimKiem { get; set; }
        // ✅ Thêm property để lưu đánh giá
        public List<DanhGiaPhong> DanhGiaList { get; set; } = new List<DanhGiaPhong>();
        
    }
}
