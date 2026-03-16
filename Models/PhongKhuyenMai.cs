namespace DACN.Models
{
    public class PhongKhuyenMai
    {
        public int MaPhong { get; set; }
        public Phong Phong { get; set; }

        public int MaKhuyenMai { get; set; }
        public KhuyenMai KhuyenMai { get; set; }
    }
}
