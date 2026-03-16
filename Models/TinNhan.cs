using System;

namespace DACN.Models
{
    public class TinNhan
    {
        public int Id { get; set; }
        public int CuocTroChuyenId { get; set; }
        public string GuiBoiRole { get; set; } = ""; // "KhachHang" | "NhanVien" | "Admin"
        public string GuiBoiName { get; set; } = ""; // tên hiển thị
        public string NoiDung { get; set; } = "";
        public DateTime ThoiGian { get; set; } = DateTime.UtcNow;

        public virtual CuocTroChuyen CuocTroChuyen { get; set; }
    }
}
