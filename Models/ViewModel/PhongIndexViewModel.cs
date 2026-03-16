namespace DACN.Models.ViewModel
{
    public class PhongIndexViewModel
    {
        public List<Phong> DanhSachPhongs { get; set; }
        public PhanTrang PhanTrang { get; set; }
        // ⭐ THÔNG TIN TÌM KIẾM MỚI ⭐
        public DateTime? NgayDen { get; set; } = DateTime.Today; // Mặc định là hôm nay
        public DateTime? NgayTra { get; set; } = DateTime.Today.AddDays(1); // Mặc định là ngày mai
        public int SoNguoi { get; set; } = 1;
        public string LoaiPhong { get; set; }

        // ⭐ Danh sách Loại Phòng để hiển thị trong Dropdown
        public List<string> DanhSachLoaiPhong { get; set; }
    }
}
