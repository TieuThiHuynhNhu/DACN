using DACN.Models;
using DACN.Models.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DACN.Controllers
{
    public class DichVuController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DichVuController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Hiển thị danh sách dịch vụ
        [AllowAnonymous]
        public async Task<IActionResult> Index(string tuKhoa, int page = 1) // Thêm tham số tuKhoa và page
        {
            int pageSize = 6; // Đổi từ 5 thành 6 để phù hợp với bố cục 3 cột (nếu cần)

            var query = _context.DichVu
                .Where(d => d.TrangThai == "ConHoatDong")
                .AsQueryable(); // Khởi tạo IQueryable

            // ⭐ ÁP DỤNG LOGIC TÌM KIẾM ⭐
            if (!string.IsNullOrEmpty(tuKhoa))
            {
                // Chuyển đổi từ khóa sang chữ thường để tìm kiếm không phân biệt chữ hoa/thường
                string lowerTuKhoa = tuKhoa.ToLower();

                // Lọc theo TênDichVu HOẶC MoTa
                query = query.Where(d =>
                    d.TenDichVu.ToLower().Contains(lowerTuKhoa) ||
                    (d.MoTa != null && d.MoTa.ToLower().Contains(lowerTuKhoa))
                );
            }

            // 1. Phân trang
            var totalItems = await query.CountAsync();
            var dichVus = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var phanTrang = new PhanTrang(totalItems, page, pageSize);

            var model = new DichVuIndexViewModel
            {
                DanhSachDichVus = dichVus,
                PhanTrang = phanTrang,
                // ⭐ Gán lại từ khóa để giữ trạng thái trong form ⭐
                TuKhoa = tuKhoa
            };

            return View(model);
        }

        // Chi tiết dịch vụ
        [AllowAnonymous]
        public IActionResult ChiTiet(int id)
        {
            var dichVu = _context.DichVu.FirstOrDefault(d => d.MaDichVu == id);
            if (dichVu == null) return NotFound();
            return View(dichVu);
        }

        // GET: Form đặt dịch vụ
        [Authorize(Roles = "KhachHang")]
        public IActionResult Dat(int maDichVu)
        {
            var dichVu = _context.DichVu.FirstOrDefault(d => d.MaDichVu == maDichVu);
            if (dichVu == null) return NotFound();

            var model = new DatDichVuViewModel
            {
                MaDichVu = maDichVu,
                NgaySuDung = DateTime.Today,
                KhungGio = TimeSpan.Parse("08:00"),
                SoLuong = 1
            };

            ViewBag.DichVu = dichVu;
            return View(model);
        }

        // POST: Đặt dịch vụ
        [HttpPost]
        [Authorize(Roles = "KhachHang")]
        public IActionResult Dat(DatDichVuViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var dvInvalid = _context.DichVu.FirstOrDefault(d => d.MaDichVu == model.MaDichVu);
                ViewBag.DichVu = dvInvalid;
                return View(model);
            }

            var tenDangNhap = HttpContext.Session.GetString("TenDangNhap");
            var khach = _context.KhachHang.FirstOrDefault(k => k.TenDangNhap == tenDangNhap);
            var dichVu = _context.DichVu.FirstOrDefault(d => d.MaDichVu == model.MaDichVu);
            if (khach == null || dichVu == null) return NotFound();

            var datDichVu = new DatDichVu
            {
                MaDichVu = model.MaDichVu,
                MaKhachHang = khach.MaKhachHang,

                NgaySuDung = model.NgaySuDung,
                KhungGio = model.KhungGio,
                SoLuong = model.SoLuong,

                // Nếu Giá dịch vụ null, dùng 0
                ThanhTien = model.SoLuong * (dichVu.GiaDichVu ?? 0m),

                TrangThai = "DatThanhCong",

                // GhiChu có thể null, nhưng model yêu cầu, nên gán string rỗng nếu null
                GhiChu = model.GhiChu ?? ""
            };


            _context.DatDichVu.Add(datDichVu);
            _context.SaveChanges();

            // Chuẩn bị model cho view xác nhận
            var viewModel = new XacNhanDichVuViewModel
            {
                KhachHang = khach,
                DichVu = dichVu,
                NgaySuDung = model.NgaySuDung,
                KhungGio = model.KhungGio,
                SoLuong = model.SoLuong,
                // Khi tính ThanhTien
                ThanhTien = model.SoLuong * (dichVu.GiaDichVu ?? 0m),

            };

            return View("XacNhanDichVu", viewModel);
        }

        // Lịch sử đặt dịch vụ của khách
        [Authorize(Roles = "KhachHang")]
        public IActionResult LichSuDatLich()
        {
            var tenDangNhap = HttpContext.Session.GetString("TenDangNhap");
            var khach = _context.KhachHang.FirstOrDefault(k => k.TenDangNhap == tenDangNhap);
            if (khach == null) return RedirectToAction("DangNhap", "TaiKhoan");

            var lichSu = _context.DatDichVu
                .Include(d => d.DichVu)
                .Where(d => d.MaKhachHang == khach.MaKhachHang)
                .OrderByDescending(d => d.NgaySuDung)
                .ToList();

            return View(lichSu);
        }

       

    }
}
