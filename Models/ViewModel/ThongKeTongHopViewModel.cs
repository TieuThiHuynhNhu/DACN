using DACN.Models;
namespace DACN.Models.ViewModel
{
    public class ThongKeTongHopViewModel
    {
        public List<DatPhong> DatPhongs { get; set; } = new();
        public List<DonThanhToanDichVu> DonDichVus { get; set; } = new();
        public List<PhieuHuHai> PhieuHuHais { get; set; } = new();
        public List<ChamCong> ChamCongs { get; set; } = new();

        public int? SelectedMonth { get; set; }
        public int? SelectedYear { get; set; }
        public List<DatDichVu> LichSuDichVu { get; internal set; }
    }
}