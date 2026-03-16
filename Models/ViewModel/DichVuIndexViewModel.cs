

using System.Collections.Generic;
namespace DACN.Models.ViewModel
{
    public class DichVuIndexViewModel
    {
        public List<DichVu> DanhSachDichVus { get; set; }
        public PhanTrang PhanTrang { get; set; }
        // ⭐ THUỘC TÍNH TÌM KIẾM MỚI ⭐
        public string TuKhoa { get; set; }
    }
}
