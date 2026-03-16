using System;
using System.Collections.Generic;

namespace DACN.Models.ViewModel
{
    public class HomeIndexViewModel
    {
        public List<SliderModel> Sliders { get; set; } = new();
        public List<KhuyenMai> KhuyenMais { get; set; } = new();
        public List<Phong> PhongList { get; set; } = new();
        public List<DichVu> DichVuList { get; set; } = new();
        public List<string> LoaiPhongs { get; set; } = new();
        // Thêm đánh giá khách sạn
        public double DiemDanhGia { get; set; }  // ví dụ: 9.2
        public int SoLuongDanhGia { get; set; }  // ví dụ: 3000
    }

}
