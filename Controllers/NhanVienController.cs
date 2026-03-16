using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using DACN.Models;

namespace DACN.Controllers
{
    [Authorize(Roles = "Admin,NhanVien")]
    public class NhanVienController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;

        public NhanVienController(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // ======================= QUẢN LÝ NHÂN VIÊN (Admin) =======================

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index()
        {
            var list = await _context.NhanVien.ToListAsync();
            return View(list);
        }

        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(NhanVien model, IFormFile? HinhAnh)
        {
            if (!ModelState.IsValid) return View(model);

            // Upload ảnh
            if (HinhAnh != null && HinhAnh.Length > 0)
            {
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(HinhAnh.FileName);
                var path = Path.Combine(_env.WebRootPath, "images/nhanvien", fileName);
                using var stream = new FileStream(path, FileMode.Create);
                await HinhAnh.CopyToAsync(stream);
                model.HinhAnhKhuonMat = "/images/nhanvien/" + fileName;
            }

            // Sinh mã nhân viên tự động (int)
            var soLuong = await _context.NhanVien.CountAsync() + 1;
            model.MaNhanVien = soLuong;

            _context.NhanVien.Add(model);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id)
        {
            var nhanVien = await _context.NhanVien.FindAsync(id);
            if (nhanVien == null) return NotFound();
            return View(nhanVien);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, NhanVien model, IFormFile? HinhAnh)
        {
            var nhanVien = await _context.NhanVien.FindAsync(id);
            if (nhanVien == null) return NotFound();
            if (!ModelState.IsValid) return View(model);

            nhanVien.HoTen = model.HoTen;
            nhanVien.GioiTinh = model.GioiTinh;
            nhanVien.ViTri = model.ViTri;
            nhanVien.CaLamViec = model.CaLamViec;
            nhanVien.TaiKhoanDangNhap = model.TaiKhoanDangNhap;
            nhanVien.MatKhau = model.MatKhau;
            nhanVien.SoDienThoai = model.SoDienThoai;
            nhanVien.Email = model.Email;
            nhanVien.DiaChi = model.DiaChi;
            nhanVien.NgaySinh = model.NgaySinh;

            if (HinhAnh != null && HinhAnh.Length > 0)
            {
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(HinhAnh.FileName);
                var path = Path.Combine(_env.WebRootPath, "images/nhanvien", fileName);
                using var stream = new FileStream(path, FileMode.Create);
                await HinhAnh.CopyToAsync(stream);
                nhanVien.HinhAnhKhuonMat = "/images/nhanvien/" + fileName;
            }

            _context.Update(nhanVien);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var nv = await _context.NhanVien.FindAsync(id);
            if (nv == null) return NotFound();

            _context.NhanVien.Remove(nv);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // ======================= THÔNG TIN CÁ NHÂN (Nhân viên) =======================

        [Authorize(Roles = "NhanVien")]
        [HttpGet]
        public IActionResult ThongTin()
        {
            var username = HttpContext.Session.GetString("TenDangNhap");
            if (string.IsNullOrEmpty(username))
            {
                return RedirectToAction("DangNhap", "TaiKhoan");
            }

            var nhanVien = _context.NhanVien.FirstOrDefault(nv => nv.TaiKhoanDangNhap == username);

            if (nhanVien == null)
            {
                // Chưa có thông tin => tạo object trống, gán username
                nhanVien = new NhanVien
                {
                    TaiKhoanDangNhap = username
                };
            }

            return View(nhanVien);
        }

        [HttpPost]
        [Authorize(Roles = "NhanVien")]
        public IActionResult ThongTin(NhanVien model, IFormFile? HinhAnh)
        {
            var username = HttpContext.Session.GetString("TenDangNhap");
            if (string.IsNullOrEmpty(username))
            {
                return RedirectToAction("DangNhap", "TaiKhoan");
            }

            var nhanVien = _context.NhanVien.FirstOrDefault(nv => nv.TaiKhoanDangNhap == username);

            // Upload ảnh nếu có
            if (HinhAnh != null && HinhAnh.Length > 0)
            {
                var fileName = Path.GetFileName(HinhAnh.FileName);
                var path = Path.Combine("wwwroot/images/nhanvien", fileName);
                using (var stream = new FileStream(path, FileMode.Create))
                {
                    HinhAnh.CopyTo(stream);
                }
                model.HinhAnhKhuonMat = "/images/nhanvien/" + fileName;
            }

            if (nhanVien == null)
            {
                // Lần đầu -> thêm mới
                _context.NhanVien.Add(model);
            }
            else
            {
                // Đã có -> cập nhật
                nhanVien.HoTen = model.HoTen;
                nhanVien.GioiTinh = model.GioiTinh;
                nhanVien.ViTri = model.ViTri;
                nhanVien.CaLamViec = model.CaLamViec;
                nhanVien.MatKhau = model.MatKhau;
                nhanVien.SoDienThoai = model.SoDienThoai;
                nhanVien.Email = model.Email;
                nhanVien.DiaChi = model.DiaChi;
                nhanVien.NgaySinh = model.NgaySinh;

                if (!string.IsNullOrEmpty(model.HinhAnhKhuonMat))
                    nhanVien.HinhAnhKhuonMat = model.HinhAnhKhuonMat;

                _context.NhanVien.Update(nhanVien);
            }

            _context.SaveChanges();
            ViewBag.Message = "Cập nhật thông tin thành công!";
            return View(model);
        }
        public async Task<IActionResult> Chat()
        {
            var messages = await _context.CuocTroChuyens
                .OrderBy(m => m.BatDauLuc)
                .ToListAsync();

            return View(messages);
        }
    }
}
