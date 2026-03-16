using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DACN.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DACN.Controllers
{
    public class QuanLyTaiKhoanController : Controller
    {
        private readonly ApplicationDbContext _context;

        public QuanLyTaiKhoanController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Phương thức chính: Hiển thị danh sách tài khoản, có hỗ trợ Tìm kiếm và Lọc
        public async Task<IActionResult> Index(string searchString, string roleFilter)
        {
            // Bắt đầu query từ bảng TaiKhoan
            var taiKhoans = from t in _context.TaiKhoan select t;

            // 1. Xử lý Tìm kiếm theo Tên Đăng Nhập
            if (!string.IsNullOrEmpty(searchString))
            {
                // Lọc tài khoản có tên đăng nhập chứa chuỗi tìm kiếm (không phân biệt chữ hoa/thường)
                taiKhoans = taiKhoans.Where(t => t.TenDangNhap.Contains(searchString));
                ViewData["CurrentSearch"] = searchString;
            }

            // 2. Xử lý Lọc theo Quyền Truy Cập
            // Chỉ định các quyền cố định để hiển thị trong Dropdown Lọc
            ViewBag.Roles = new List<string> { "Admin", "NhanVien", "KhachHang" };

            if (!string.IsNullOrEmpty(roleFilter) && roleFilter != "Tất cả")
            {
                taiKhoans = taiKhoans.Where(t => t.QuyenTruyCap == roleFilter);
                ViewData["CurrentRoleFilter"] = roleFilter;
            }
            else
            {
                ViewData["CurrentRoleFilter"] = "Tất cả";
            }

            // Trả về View với danh sách tài khoản đã lọc/tìm kiếm
            return View(await taiKhoans.ToListAsync());
        }

        // Lọc: Hiển thị các tài khoản đang bị chặn
        public async Task<IActionResult> TaiKhoanBiChan()
        {
            var taiKhoans = await _context.TaiKhoan
                .Where(t => t.BiChan == true)
                .ToListAsync();
            // Tái sử dụng View Index để hiển thị kết quả
            return View("Index", taiKhoans);
        }

        // Lọc: Hiển thị các tài khoản đang hoạt động
        public async Task<IActionResult> TaiKhoanHoatDong()
        {
            var taiKhoans = await _context.TaiKhoan
                .Where(t => t.BiChan == false)
                .ToListAsync();
            // Tái sử dụng View Index để hiển thị kết quả
            return View("Index", taiKhoans);
        }

        // GET: Hiển thị form tạo tài khoản mới (cho Nhân viên/Admin)
        public IActionResult Create()
        {
            // Admin có quyền tạo tài khoản với quyền cao (Không phải Khách hàng)
            ViewBag.AvailableRoles = new List<string> { "NhanVien", "Admin" };
            return View();
        }

        // POST: Xử lý lưu tài khoản mới vào database
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TaiKhoan taiKhoanModel, string MatKhauClearText)
        {
            // Danh sách quyền cần thiết để hiển thị lại nếu có lỗi
            ViewBag.AvailableRoles = new List<string> { "Nhân viên", "Quản trị viên" };

            // Kiểm tra tính hợp lệ cơ bản
            if (string.IsNullOrEmpty(taiKhoanModel.TenDangNhap) || string.IsNullOrEmpty(MatKhauClearText))
            {
                ModelState.AddModelError("", "Vui lòng nhập đầy đủ Tên đăng nhập và Mật khẩu.");
                return View(taiKhoanModel);
            }

            // Kiểm tra TenDangNhap đã tồn tại chưa
            if (await _context.TaiKhoan.AnyAsync(t => t.TenDangNhap == taiKhoanModel.TenDangNhap))
            {
                ModelState.AddModelError("TenDangNhap", "Tên đăng nhập này đã tồn tại.");
                return View(taiKhoanModel);
            }

            // ⭐ CẢNH BÁO QUAN TRỌNG VỀ BẢO MẬT ⭐
            // TRONG THỰC TẾ, BẠN PHẢI HASH MẬT KHẨU (ví dụ: dùng BCrypt) trước khi lưu vào database!
            // Ví dụ: string hashedPassword = HashService.HashPassword(MatKhauClearText);

            var taiKhoanMoi = new TaiKhoan
            {
                TenDangNhap = taiKhoanModel.TenDangNhap,
                MatKhau = MatKhauClearText, // ⚠️ Thay thế bằng hashed password THỰC TẾ ⚠️
                QuyenTruyCap = taiKhoanModel.QuyenTruyCap,
                BiChan = false, // Mặc định không bị chặn
                BaoMat2FA = false // Mặc định không kích hoạt 2FA
            };

            _context.TaiKhoan.Add(taiKhoanMoi);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Đã tạo tài khoản '{taiKhoanMoi.TenDangNhap}' thành công với quyền '{taiKhoanMoi.QuyenTruyCap}'.";
            return RedirectToAction(nameof(Index));
        }

        // Chi tiết tài khoản
        public async Task<IActionResult> ChiTiet(string tenDangNhap)
        {
            if (string.IsNullOrEmpty(tenDangNhap)) return NotFound();

            var taiKhoan = await _context.TaiKhoan
                .FirstOrDefaultAsync(t => t.TenDangNhap == tenDangNhap);

            if (taiKhoan == null) return NotFound();

            return View(taiKhoan);
        }

        // POST: Chặn / mở chặn tài khoản
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleChan(string tenDangNhap)
        {
            var taiKhoan = await _context.TaiKhoan
                .FirstOrDefaultAsync(t => t.TenDangNhap == tenDangNhap);

            if (taiKhoan != null)
            {
                taiKhoan.BiChan = !taiKhoan.BiChan;
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Đã {(taiKhoan.BiChan ? "chặn" : "mở chặn")} tài khoản '{tenDangNhap}'.";
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: Xóa tài khoản
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Xoa(string tenDangNhap)
        {
            var taiKhoan = await _context.TaiKhoan
                .FirstOrDefaultAsync(t => t.TenDangNhap == tenDangNhap);

            if (taiKhoan != null)
            {
                _context.TaiKhoan.Remove(taiKhoan);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Đã xóa tài khoản '{tenDangNhap}' thành công.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}