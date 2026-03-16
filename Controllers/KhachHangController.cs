using DACN.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace DACN.Controllers
{
    public class KhachHangController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;

        public KhachHangController(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // ======================= HIỂN THỊ THÔNG TIN KHÁCH HÀNG =======================
        [Authorize(Roles = "KhachHang")]
        [HttpGet]
        public IActionResult ThongTin()
        {
            var username = HttpContext.Session.GetString("TenDangNhap");
            if (string.IsNullOrEmpty(username))
                return RedirectToAction("DangNhap", "TaiKhoan");

            var kh = _context.KhachHang.FirstOrDefault(k => k.TenDangNhap == username);
            if (kh == null)
                return NotFound("Không tìm thấy thông tin khách hàng.");

            // Ảnh mặc định nếu chưa có
            if (string.IsNullOrEmpty(kh.HinhAnh))
                kh.HinhAnh = "/images/khachhang/default.jpg";

            return View(kh);
        }

        // ======================= CẬP NHẬT THÔNG TIN KHÁCH HÀNG =======================
        [Authorize(Roles = "KhachHang")]
        [HttpPost]
        public IActionResult ThongTin(KhachHang model, string? MatKhauCu, string? MatKhauMoi, IFormFile? HinhAnhFile)
        {
            var username = HttpContext.Session.GetString("TenDangNhap");
            if (string.IsNullOrEmpty(username))
                return RedirectToAction("DangNhap", "TaiKhoan");

            var kh = _context.KhachHang.FirstOrDefault(k => k.TenDangNhap == username);
            if (kh == null)
                return NotFound("Không tìm thấy thông tin khách hàng.");

            // ===== Cập nhật thông tin cơ bản =====
            kh.HoTen = model.HoTen;
            kh.SoDienThoai = model.SoDienThoai;
            kh.Email = model.Email;

            // ===== Upload ảnh mới (nếu có) =====
            if (HinhAnhFile != null && HinhAnhFile.Length > 0)
            {
                var uploadsFolder = Path.Combine(_env.WebRootPath, "images/khachhang");
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                var fileName = Path.GetFileName(HinhAnhFile.FileName);
                var filePath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    HinhAnhFile.CopyTo(stream);
                }

                kh.HinhAnh = "/images/khachhang/" + fileName;
            }

            // Nếu chưa có ảnh thì gán ảnh mặc định
            if (string.IsNullOrEmpty(kh.HinhAnh))
                kh.HinhAnh = "/images/khachhang/default.jpg";

            // ===== Xử lý đổi mật khẩu =====
            if (!string.IsNullOrEmpty(MatKhauMoi))
            {
                if (string.IsNullOrEmpty(MatKhauCu))
                {
                    ViewBag.Error = "⚠️ Vui lòng nhập mật khẩu hiện tại để đổi mật khẩu.";
                    return View(kh);
                }

                // Lấy tài khoản trong bảng TaiKhoan
                var taiKhoan = _context.TaiKhoan.FirstOrDefault(t => t.TenDangNhap == username);

                if (taiKhoan == null)
                {
                    ViewBag.Error = "❌ Không tìm thấy tài khoản.";
                    return View(kh);
                }

                // So sánh mật khẩu cũ trong bảng TaiKhoan
                if (taiKhoan.MatKhau != MatKhauCu)
                {
                    ViewBag.Error = "❌ Mật khẩu cũ không đúng.";
                    return View(kh);
                }

                // Cập nhật mật khẩu mới
                kh.MatKhau = MatKhauMoi;
                taiKhoan.MatKhau = MatKhauMoi;
            }

            // ===== Lưu thay đổi =====
            _context.KhachHang.Update(kh);
            _context.SaveChanges();

            ViewBag.Message = "✅ Cập nhật thông tin thành công!";
            return View(kh);
        }
    }
}
