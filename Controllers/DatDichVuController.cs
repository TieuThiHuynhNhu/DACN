using DACN.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DACN.Controllers
{
    public class DatDichVuController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DatDichVuController(ApplicationDbContext context)
        {
            _context = context;
        }

        // -------------------------------
        // HIỂN THỊ DANH SÁCH DỊCH VỤ
        // -------------------------------
        public IActionResult Index()
        {
            var dichVus = _context.DichVu.ToList();
            return View(dichVus);
        }

        // -------------------------------
        // ĐẶT LỊCH DỊCH VỤ
        // -------------------------------
        [HttpPost]
        public IActionResult DatLich(int maDichVu, DateTime ngayDat, TimeSpan khungGio, string? ghiChu)
        {
            var tenDangNhap = HttpContext.Session.GetString("TenDangNhap");
            if (string.IsNullOrEmpty(tenDangNhap))
                return RedirectToAction("DangNhap", "TaiKhoan");

            var khachHang = _context.KhachHang.FirstOrDefault(k => k.TenDangNhap == tenDangNhap);
            if (khachHang == null)
                return Unauthorized();

            // Kiểm tra dịch vụ có tồn tại không
            var dichVu = _context.DichVu.FirstOrDefault(d => d.MaDichVu == maDichVu);
            if (dichVu == null)
                return NotFound();

            // -------------------------------
            // KIỂM TRA SỐ LƯỢNG ĐẶT TRONG CÙNG KHUNG GIỜ
            // -------------------------------
            int soLuong = _context.DatDichVu
    .Where(d => d.MaDichVu == maDichVu
        && d.NgaySuDung.HasValue
        && d.NgaySuDung.Value.Date == ngayDat.Date
        && d.KhungGio == khungGio)
    .Count();

            if (soLuong >= 5)
            {
                TempData["Error"] = "Khung giờ này đã có đủ 5 khách. Vui lòng chọn giờ khác!";
                return RedirectToAction("Index");
            }

            // -------------------------------
            // LƯU LỊCH ĐẶT
            // -------------------------------
            var dat = new DatDichVu
            {
                MaKhachHang = khachHang.MaKhachHang,
                MaDichVu = maDichVu,
                NgaySuDung = ngayDat,
                KhungGio = khungGio,
                GhiChu = ghiChu,
                TrangThai = "Đã đặt"
            };

            _context.DatDichVu.Add(dat);
            _context.SaveChanges();

            TempData["Success"] = "Đặt lịch thành công!";
            return RedirectToAction("LichSuDatDichVu");
        }

        // -------------------------------
        // LỊCH SỬ ĐẶT LỊCH
        // -------------------------------
        public IActionResult LichSuDatDichVu()
        {
            var tenDangNhap = HttpContext.Session.GetString("TenDangNhap");
            if (string.IsNullOrEmpty(tenDangNhap))
                return RedirectToAction("DangNhap", "TaiKhoan");

            var taiKhoan = _context.TaiKhoan.FirstOrDefault(t => t.TenDangNhap == tenDangNhap);
            if (taiKhoan == null)
                return Unauthorized();
            // THÊM DÒNG NÀY ĐỂ XỬ LÝ VIỆC HIỂN THỊ THÔNG TIN KHÁCH HÀNG TRONG VIEW
            ViewData["IsAdmin"] = (taiKhoan.QuyenTruyCap == "Admin");
            List<DatDichVu> lichSu;

            // -------------------------------
            // ADMIN XEM TẤT CẢ
            // -------------------------------
            if (taiKhoan.QuyenTruyCap == "Admin")
            {
                lichSu = _context.DatDichVu
                    .Include(d => d.DichVu)
                    .Include(d => d.KhachHang)
                    .OrderByDescending(d => d.NgaySuDung)
                    .ToList();
            }
            else
            {
                // Khách xem của chính họ
                var khachHang = _context.KhachHang.FirstOrDefault(k => k.TenDangNhap == tenDangNhap);
                if (khachHang == null)
                    return Unauthorized();

                lichSu = _context.DatDichVu
                    .Where(d => d.MaKhachHang == khachHang.MaKhachHang)
                    .Include(d => d.DichVu)
                    .OrderByDescending(d => d.NgaySuDung)
                    .ToList();
            }

            return View(lichSu);
        }
        // XEM CHI TIẾT LỊCH SỬ ĐẶT DỊCH VỤ / HÓA ĐƠN
        // -------------------------------
        public async Task<IActionResult> ChiTietDatDichVu(int id)
        {
            var tenDangNhap = HttpContext.Session.GetString("TenDangNhap");
            if (string.IsNullOrEmpty(tenDangNhap))
                return RedirectToAction("DangNhap", "TaiKhoan");

            var taiKhoan = await _context.TaiKhoan.FirstOrDefaultAsync(t => t.TenDangNhap == tenDangNhap);
            if (taiKhoan == null)
                return Unauthorized();

            var datDichVu = await _context.DatDichVu
                .Include(d => d.DichVu)
                .Include(d => d.KhachHang)
                .FirstOrDefaultAsync(d => d.MaDatDichVu == id);

            if (datDichVu == null)
                return NotFound();

            // Khách hàng chỉ được xem lịch của chính họ, Admin được xem tất cả
            if (taiKhoan.QuyenTruyCap != "Admin")
            {
                var khachHang = await _context.KhachHang.FirstOrDefaultAsync(k => k.TenDangNhap == tenDangNhap);
                if (khachHang == null || datDichVu.MaKhachHang != khachHang.MaKhachHang)
                {
                    return Forbid(); // Trả về lỗi 403 - Cấm truy cập nếu không phải là chủ sở hữu
                }
            }

            // Tính toán tổng tiền
            decimal tongTien = datDichVu.DichVu?.GiaDichVu ?? 0;

            ViewData["TongTien"] = tongTien;

            return View(datDichVu);
        }
        public async Task<IActionResult> InHoaDonDichVu(int id)
        {
            var tenDangNhap = HttpContext.Session.GetString("TenDangNhap");
            if (string.IsNullOrEmpty(tenDangNhap))
                return RedirectToAction("DangNhap", "TaiKhoan");

            var taiKhoan = await _context.TaiKhoan.FirstOrDefaultAsync(t => t.TenDangNhap == tenDangNhap);
            if (taiKhoan == null)
                return Unauthorized();

            var datDichVu = await _context.DatDichVu
                .Include(d => d.DichVu)
                .Include(d => d.KhachHang)
                .FirstOrDefaultAsync(d => d.MaDatDichVu == id);

            if (datDichVu == null)
                return NotFound();

            // Khách hàng chỉ được xem lịch của chính họ, Admin được xem tất cả
            if (taiKhoan.QuyenTruyCap != "Admin")
            {
                var khachHang = await _context.KhachHang.FirstOrDefaultAsync(k => k.TenDangNhap == tenDangNhap);
                if (khachHang == null || datDichVu.MaKhachHang != khachHang.MaKhachHang)
                {
                    return Forbid(); // Trả về lỗi 403 - Cấm truy cập
                }
            }

            // Tính toán tổng tiền (có thể phức tạp hơn nếu có số lượng, nhưng hiện tại chỉ lấy GiáDichVu)
            decimal tongTien = (datDichVu.DichVu?.GiaDichVu ?? 0) * (datDichVu.SoLuong > 0 ? datDichVu.SoLuong : 1);
            ViewData["TongTien"] = tongTien;

            // Sử dụng _Layout trống (nếu có) hoặc View đơn giản để in
            return View(datDichVu);
        }
    }
}
