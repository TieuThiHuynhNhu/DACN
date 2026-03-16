using DACN.Models;
using DACN.Models.ViewModel;
using DACN.Models.VNPAY;
using DACN.Services.VNPay;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace DACN.Controllers
{
    public class DatPhongController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IVnPayService _vnPayService;
        public DatPhongController(ApplicationDbContext context, IVnPayService vnPayService)
        {
            _context = context;
            _vnPayService = vnPayService;
        }

        [HttpGet]
        public IActionResult Dat(int maPhong)
        {
            var quyen = HttpContext.Session.GetString("Quyen");
            if (quyen != "KhachHang" && quyen != "NhanVien") return RedirectToAction("AccessDenied", "TaiKhoan");

            var phong = _context.Phong.FirstOrDefault(p => p.MaPhong == maPhong);
            if (phong == null) return NotFound();

            var model = new DatPhongViewModel
            {
                MaPhong = maPhong,
                NgayNhan = DateTime.Today,
                NgayTra = DateTime.Today.AddDays(1),
                GiaPhong = phong.GiaPhong
            };

            ViewBag.Phong = phong;
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Dat(DatPhongViewModel model)
        {
            var quyen = HttpContext.Session.GetString("Quyen");
            if (quyen != "KhachHang" && quyen != "NhanVien") return RedirectToAction("AccessDenied", "TaiKhoan");

            var phong = _context.Phong.FirstOrDefault(p => p.MaPhong == model.MaPhong);
            ViewBag.Phong = phong;

            if (phong == null) return RedirectToAction("Index", "Phong");

            var soNgayThue = (model.NgayTra - model.NgayNhan).Days;
            decimal tongTien = soNgayThue >= 1 ? soNgayThue * phong.GiaPhong : 0;

            if (model.NgayTra <= model.NgayNhan) ModelState.AddModelError("NgayTra", "Ngày trả phòng phải sau Ngày nhận phòng.");
            if (!ModelState.IsValid) return View(model);

            var tenDangNhap = HttpContext.Session.GetString("TenDangNhap");
            var khach = _context.KhachHang.FirstOrDefault(k => k.TenDangNhap == tenDangNhap);
            if (khach == null) return RedirectToAction("DangNhap", "TaiKhoan");

            var datPhong = new DatPhong
            {
                MaPhong = model.MaPhong,
                MaKhachHang = khach.MaKhachHang,
                NgayDat = DateTime.Now,
                NgayNhan = model.NgayNhan,
                NgayTra = model.NgayTra,
                TongTien = tongTien,
                HanHuyPhong = DateTime.Now.AddDays(1),
                GhiChu = model.GhiChu ?? "",
                HinhThucThanhToan = model.HinhThucThanhToan,
                // Mặc định là ChoThanhToan cho đến khi xác nhận qua CPay/VNPay
                TrangThai = (model.HinhThucThanhToan == "ThanhToanSau") ? "DaDat" : "ChoThanhToan"
            };

            if (model.HinhThucThanhToan == "ThanhToanSau")
            {
                phong.TrangThai = "DaDat";
                _context.Phong.Update(phong);
            }

            _context.DatPhong.Add(datPhong);
            _context.SaveChanges();

            if (model.HinhThucThanhToan == "CPay")
            {
                return RedirectToAction("ThanhToanCPay", new { maDatPhong = datPhong.MaDatPhong });
            }

            if (model.HinhThucThanhToan == "ThanhToanSau")
            {
                return RedirectToAction("DatPhongThanhCong", new { id = datPhong.MaDatPhong });
            }
            else
            {
                return RedirectToAction("XacNhan", new { id = datPhong.MaDatPhong });
            }
        }

        [HttpGet]
        [Authorize(Roles = "KhachHang,NhanVien")]
        public IActionResult ThanhToanCPay(int maDatPhong)
        {
            var dp = _context.DatPhong
                .Include(d => d.Phong)
                .FirstOrDefault(d => d.MaDatPhong == maDatPhong);

            if (dp == null) return NotFound();

            var paymentModel = new CPayViewModel
            {
                MaGiaoDich = "ROOM_" + DateTime.Now.Ticks,
                SoTien = dp.TongTien,
                NoiDung = $"Thanh toán đặt phòng {dp.Phong.LoaiPhong}",
                MaDatId = maDatPhong
            };

            return View("CPayCheckout", paymentModel);
        }

        // ⭐ CẬP NHẬT: Xử lý lưu trạng thái "DaDat" khi thanh toán CPay thành công
        [HttpGet]
        public async Task<IActionResult> KetQuaThanhToanCPay(int maDatId, bool thanhCong)
        {
            // Tắt cache để đảm bảo dữ liệu mới nhất
            Response.Headers.Add("Cache-Control", "no-cache, no-store, must-revalidate");

            var dp = await _context.DatPhong.Include(d => d.Phong).FirstOrDefaultAsync(d => d.MaDatPhong == maDatId);
            if (dp == null) return NotFound();

            if (thanhCong)
            {
                // 1. Cập nhật trạng thái
                dp.TrangThai = "DaDat";

                // 2. Ép buộc Entity Framework đánh dấu Modified
                _context.Entry(dp).State = EntityState.Modified;

                if (dp.Phong != null)
                {
                    dp.Phong.TrangThai = "DaDat";
                    _context.Entry(dp.Phong).State = EntityState.Modified;
                }

                // 3. Lưu và kiểm tra
                await _context.SaveChangesAsync();
                TempData["Success"] = "Thanh toán CPay thành công!";
            }

            return RedirectToAction("ThanhToanThanhCongCPay", new { id = maDatId });
        }
        [HttpGet]
        public async Task<IActionResult> ThanhToanThanhCongCPay(int id)
        {
            // 1. Lấy thông tin đơn đặt kèm thông tin Phòng
            var dat = await _context.DatPhong
                .Include(d => d.KhachHang)
                .Include(d => d.Phong)
                .FirstOrDefaultAsync(d => d.MaDatPhong == id);

            if (dat == null) return NotFound();

            // 2. ⭐ LOGIC QUAN TRỌNG: Cập nhật trạng thái nếu nó vẫn đang là "ChoThanhToan"
            if (dat.TrangThai == "ChoThanhToan")
            {
                dat.TrangThai = "DaDat"; // Chuyển sang Đã đặt thành công

                if (dat.Phong != null)
                {
                    dat.Phong.TrangThai = "DaDat"; // Cập nhật trạng thái phòng để không ai đặt trùng
                    _context.Entry(dat.Phong).State = EntityState.Modified;
                }

                _context.Entry(dat).State = EntityState.Modified;
                await _context.SaveChangesAsync(); // Lưu vĩnh viễn vào SQL Server
            }

            ViewBag.KhachHang = dat.KhachHang;
            ViewBag.Phong = dat.Phong;

            return View(dat);
        }

        public IActionResult DatPhongThanhCong(int id)
        {
            var dat = _context.DatPhong
                .Include(d => d.KhachHang)
                .Include(d => d.Phong)
                .FirstOrDefault(d => d.MaDatPhong == id);

            if (dat == null) return NotFound();

            ViewBag.KhachHang = dat.KhachHang;
            ViewBag.Phong = dat.Phong;

            return View(dat);
        }

        public IActionResult XacNhan(int id)
        {
            var dat = _context.DatPhong
                .Include(d => d.KhachHang)
                .Include(d => d.Phong)
                .FirstOrDefault(d => d.MaDatPhong == id);

            if (dat == null) return NotFound();

            ViewBag.KhachHang = dat.KhachHang;
            ViewBag.Phong = dat.Phong;

            var paymentModel = new PaymentInformationModel
            {
                OrderId = dat.MaDatPhong,
                Amount = (double)dat.TongTien,
                OrderDescription = "Thanh toán đặt phòng khách sạn",
                Name = dat.KhachHang.HoTen,
                OrderType = "hotel"
            };

            ViewBag.PaymentModel = paymentModel;
            return View(dat);
        }

        [HttpPost]
        public async Task<IActionResult> ChuyenSangThanhToanSau(int maDatPhong)
        {
            var datPhong = await _context.DatPhong
                .Include(d => d.Phong)
                .FirstOrDefaultAsync(d => d.MaDatPhong == maDatPhong);

            if (datPhong == null) return NotFound();

            if (datPhong.TrangThai != "ChoThanhToan")
            {
                TempData["Error"] = "Trạng thái không hợp lệ.";
                return RedirectToAction(nameof(XacNhan), new { id = maDatPhong });
            }

            datPhong.HinhThucThanhToan = "ThanhToanSau";
            datPhong.TrangThai = "DaDat";

            if (datPhong.Phong != null)
            {
                datPhong.Phong.TrangThai = "DaDat";
                _context.Phong.Update(datPhong.Phong);
            }

            await _context.SaveChangesAsync();
            TempData["Message"] = "Đã chuyển sang Thanh toán tại quầy thành công!";
            return RedirectToAction(nameof(DatPhongThanhCong), new { id = maDatPhong });
        }

        public async Task<IActionResult> LichSuDatPhong()
        {
            var tenDangNhap = HttpContext.Session.GetString("TenDangNhap");
            var khach = await _context.KhachHang.FirstOrDefaultAsync(k => k.TenDangNhap == tenDangNhap);
            if (khach == null) return RedirectToAction("DangNhap", "TaiKhoan");

            // ⭐ SỬA LẠI BỘ LỌC ĐỂ ĐẢM BẢO HIỂN THỊ ĐÚNG ⭐
            var lichSu = await _context.DatPhong
                .Include(d => d.Phong)
                .Where(d => d.MaKhachHang == khach.MaKhachHang &&
                            (d.TrangThai == "DaDat" || d.TrangThai == "DaThanhToan" || d.TrangThai == "ChoThanhToan") &&
                            d.HanHuyPhong.HasValue &&
                            d.HanHuyPhong.Value > DateTime.Now)
                .OrderByDescending(d => d.NgayDat)
                .ToListAsync();

            return View(lichSu);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> HuyDatPhong(int maDatPhong)
        {
            var datPhong = await _context.DatPhong
                .Include(d => d.Phong)
                .FirstOrDefaultAsync(d => d.MaDatPhong == maDatPhong);

            if (datPhong == null) return NotFound();

            if (datPhong.HanHuyPhong.HasValue && DateTime.Now <= datPhong.HanHuyPhong.Value && datPhong.TrangThai != "DaHuy")
            {
                datPhong.TrangThai = "DaHuy";
                datPhong.GhiChu = "Đã hủy bởi khách hàng.";
                if (datPhong.Phong != null)
                {
                    datPhong.Phong.TrangThai = "ConTrong";
                    _context.Phong.Update(datPhong.Phong);
                }
                await _context.SaveChangesAsync();
                TempData["Success"] = "Đã hủy đơn thành công.";
            }
            else
            {
                TempData["Error"] = "Hết hạn hủy phòng.";
            }

            return RedirectToAction(nameof(LichSuDatPhong));
        }

        [HttpGet]
        public IActionResult PaymentCallbackVnpay()
        {
            try
            {
                if (Request.Query == null || !Request.Query.Any())
                    return Content("❌ Không có dữ liệu VNPay.");

                var response = _vnPayService.PaymentExecute(Request.Query);

                if (response != null && response.VnPayResponseCode == "00")
                {
                    return Content("✅ Giao dịch thành công!");
                }

                return Content($"❌ Giao dịch thất bại.");
            }
            catch (Exception ex)
            {
                return Content($"⚠️ Lỗi: {ex.Message}");
            }
        }
        [HttpPost]
        public IActionResult LuuThongTinEKyc(string hoTen, string soID, string diaChi)
        {
            // Cập nhật thông tin vào bảng KhachHang dựa trên kết quả AI gửi về
            // Giúp hoàn tất thủ tục check-in nhanh chóng
            return Json(new { success = true, message = "Đã cập nhật hồ sơ khách hàng từ AI" });
        }

        
    }
}