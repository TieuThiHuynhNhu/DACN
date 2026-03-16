using DACN.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;

namespace DACN.Controllers
{
    public class TaiKhoanController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TaiKhoanController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult DangNhap()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> DangNhap(string tenDangNhap, string matKhau)
        {
            var tk = _context.TaiKhoan.FirstOrDefault(t => t.TenDangNhap == tenDangNhap && t.MatKhau == matKhau);

            if (tk == null)
            {
                ViewBag.ThongBao = "Sai tên đăng nhập hoặc mật khẩu";
                return View();
            }

            // Kiểm tra nếu bị chặn
            if (tk.BiChan)
            {
                ViewBag.ThongBao = "Tài khoản của bạn đã bị chặn. Vui lòng liên hệ quản trị viên!";
                return View();
            }

            // Tạo danh sách Claims
            var claims = new List<Claim>
    {
        new Claim(ClaimTypes.Name, tk.TenDangNhap),
        new Claim(ClaimTypes.Role, tk.QuyenTruyCap)
    };

            var identity = new ClaimsIdentity(claims, "Cookies");
            var principal = new ClaimsPrincipal(identity);

            // Đăng nhập bằng Cookie
            await HttpContext.SignInAsync("Cookies", principal);

            // Ghi session nếu cần dùng song song
            HttpContext.Session.SetString("TenDangNhap", tk.TenDangNhap);
            HttpContext.Session.SetString("Quyen", tk.QuyenTruyCap);
            // Lấy mã khách hàng
            if (tk.QuyenTruyCap == "KhachHang")
            {
                var khach = _context.KhachHang
                    .FirstOrDefault(k => k.TenDangNhap == tk.TenDangNhap);

                if (khach != null)
                    HttpContext.Session.SetInt32("MaKh", khach.MaKhachHang);
            }


            return RedirectToAction("Index", "Home");
        }


        public async Task<IActionResult> DangXuat()
        {
            await HttpContext.SignOutAsync("Cookies");
            HttpContext.Session.Clear();
            return RedirectToAction("DangNhap");
        }

        [HttpGet]
        public IActionResult DangKy()
        {
            return View();
        }

        [HttpPost]
        public IActionResult DangKy(KhachHang model)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                              .SelectMany(v => v.Errors)
                              .Select(e => e.ErrorMessage);

                ViewBag.Loi = string.Join(" | ", errors);
                return View(model);
            }
            if (!ModelState.IsValid) return View(model);

            // KIỂM TRA EMAIL
            if (_context.KhachHang.Any(x => x.Email == model.Email))
            {
                ModelState.AddModelError("", "Email đã được sử dụng!");
                return View(model);
            }

            // KIỂM TRA TÊN ĐĂNG NHẬP
            if (_context.KhachHang.Any(x => x.TenDangNhap == model.TenDangNhap))
            {
                ModelState.AddModelError("", "Tên đăng nhập đã tồn tại!");
                return View(model);
            }

            // LƯU KHÁCH HÀNG
            _context.KhachHang.Add(model);
            _context.SaveChanges();

            // TẠO TÀI KHOẢN
            var taiKhoan = new TaiKhoan
            {
                TenDangNhap = model.TenDangNhap,
                MatKhau = model.MatKhau,
                QuyenTruyCap = "KhachHang",
                BaoMat2FA = false
            };

            _context.TaiKhoan.Add(taiKhoan);
            _context.SaveChanges();
            

            return RedirectToAction("DangNhap");
        }


        [HttpGet]
        public IActionResult XacThuc2FA()
        {
            return View();
        }

        [HttpPost]
        public IActionResult XacThuc2FA(string MaXacThuc)
        {
            var maHopLe = HttpContext.Session.GetString("Ma2FA");
            if (MaXacThuc == maHopLe)
            {
                return RedirectToAction("Index", "Home");
            }
            ViewBag.ThongBao = "Mã không đúng!";
            return View();
        }

        [HttpPost]
        public IActionResult QuenMatKhau(string TenDangNhap)
        {
            var tk = _context.TaiKhoan.FirstOrDefault(t => t.TenDangNhap == TenDangNhap);
            if (tk != null)
            {
                ViewBag.MatKhauMoi = "123456";
            }
            else ViewBag.ThongBao = "Không tìm thấy tài khoản";

            return View();
        }

        [HttpGet]
        public IActionResult KhongCoQuyen()
        {
            return View("KhongCoQuyen");
        }

       
    }
}
