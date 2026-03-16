using DACN.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

public class DanhGiaController : Controller
{
    private readonly ApplicationDbContext _context;

    public DanhGiaController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpPost]
    public IActionResult Them(int MaPhong, string NoiDung)
    {
        string tenDangNhap = HttpContext.Session.GetString("TenDangNhap");
        if (string.IsNullOrEmpty(tenDangNhap))
            return RedirectToAction("DangNhap", "TaiKhoan");

        var kh = _context.KhachHang.FirstOrDefault(k => k.TenDangNhap == tenDangNhap);
        if (kh == null) return Unauthorized();

        var dg = new DanhGiaPhong
        {
            MaPhong = MaPhong,
            MaKhachHang = kh.MaKhachHang,
            NoiDung = NoiDung,
            NgayDanhGia = DateTime.Now
        };

        _context.DanhGiaPhongs.Add(dg);
        _context.SaveChanges();

        TempData["Success"] = "Đã gửi đánh giá!";
        return RedirectToAction("Index", "LichSuDat");
    }
    [HttpPost]
    [Authorize(Roles = "KhachHang")]
    public IActionResult LuuDanhGia(int MaPhong, int SoSao, string NoiDung)
    {
        var maKhachHang = HttpContext.Session.GetInt32("MaKh");
        if (maKhachHang == null)
            return RedirectToAction("DangNhap", "TaiKhoan");

        // kiểm tra có đánh giá chưa
        var dg = _context.DanhGiaPhongs
            .FirstOrDefault(d => d.MaPhong == MaPhong && d.MaKhachHang == maKhachHang);

        if (dg == null)
        {
            // thêm mới
            dg = new DanhGiaPhong
            {
                MaPhong = MaPhong,
                MaKhachHang = maKhachHang.Value,
                SoSao = SoSao,
                NoiDung = NoiDung,
                NgayDanhGia = DateTime.Now
            };

            _context.DanhGiaPhongs.Add(dg);
        }
        else
        {
            // cập nhật đánh giá
            dg.SoSao = SoSao;
            dg.NoiDung = NoiDung;
            dg.NgayDanhGia = DateTime.Now;

            _context.DanhGiaPhongs.Update(dg);
        }

        _context.SaveChanges();

        return RedirectToAction("Index", "LichSuDat"); // quay lại lịch sử đặt phòng
    }
}
