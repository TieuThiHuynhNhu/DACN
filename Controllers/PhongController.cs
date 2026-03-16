using DACN.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DACN.Models.ViewModel;

namespace DACN.Controllers
{
    public class PhongController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;

        public PhongController(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        [AllowAnonymous]
        public async Task<IActionResult> Index(DateTime? ngayDen, DateTime? ngayTra, int soNguoi = 1, string loaiPhong = null, int page = 1)
        {
            int pageSize = 6;

            // Lấy danh sách loại phòng cho dropdown trong form tìm kiếm
            var danhSachLoaiPhong = await _context.Phong
                                                  .Select(p => p.LoaiPhong)
                                                  .Distinct()
                                                  .ToListAsync();

            // 1. Chuẩn bị Query gốc
            var query = _context.Phong.AsQueryable();

            // 2. ⭐ LOGIC LỌC KẾT HỢP ⭐

            // Khởi tạo ngày mặc định nếu chưa có (dùng cho tìm kiếm lần đầu)
            DateTime finalNgayDen = ngayDen ?? DateTime.Today;
            DateTime finalNgayTra = ngayTra ?? DateTime.Today.AddDays(1);

            // BƯỚC LỌC 1: Lọc phòng không bị đặt trong khoảng thời gian
            if (finalNgayDen >= DateTime.Today && finalNgayTra > finalNgayDen)
            {
                // Lấy danh sách Mã phòng đã bị đặt trong khoảng NgayDen - NgayTra
                var maPhongDaDat = await _context.DatPhong
                    .Where(dp => dp.NgayNhan < finalNgayTra && dp.NgayTra > finalNgayDen &&
                                 // Chỉ lọc các trạng thái đã đặt/chờ thanh toán (nếu áp dụng)
                                 (dp.TrangThai == "DaDat" || dp.TrangThai == "ChoThanhToan"))
                    .Select(dp => dp.MaPhong)
                    .Distinct()
                    .ToListAsync();

                // Loại trừ các phòng đã đặt
                query = query.Where(p => !maPhongDaDat.Contains(p.MaPhong));
            }

            // BƯỚC LỌC 2: Lọc theo Loại phòng
            if (!string.IsNullOrEmpty(loaiPhong) && loaiPhong != "tất cả")
            {
                query = query.Where(p => p.LoaiPhong == loaiPhong);
            }

            // BƯỚC LỌC 3: Lọc theo Số người (Nếu cột SoNguoiToiDa có trong bảng Phong)
            // Nếu bảng Phong của bạn có cột SoNguoiToiDa, bạn có thể mở lại dòng sau:
            // query = query.Where(p => p.SoNguoiToiDa >= soNguoi); 

            // 3. Phân trang
            var totalItems = await query.CountAsync();
            var phongs = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Include(p => p.PhongKhuyenMais)
                    .ThenInclude(pk => pk.KhuyenMai)
                .ToListAsync();

            // 4. Xử lý khuyến mãi, đánh giá (Logic giữ nguyên)
            var danhSachPhongDaXuLy = phongs.Select(p =>
            {
                // Lấy đánh giá
                var ratings = _context.DanhGiaPhongs.Where(d => d.MaPhong == p.MaPhong).ToList();
                p.AverageStars = ratings.Any() ? ratings.Average(d => d.SoSao) : 0;
                p.TotalReviews = ratings.Count;

                // Lấy khuyến mãi cao nhất đang áp dụng
                var km = p.PhongKhuyenMais
                    .Where(pk => pk.KhuyenMai.NgayBatDau <= DateTime.Now &&
                                 pk.KhuyenMai.NgayKetThuc >= DateTime.Now)
                    .OrderByDescending(pk => pk.KhuyenMai.PhanTramGiam)
                    .Select(pk => pk.KhuyenMai)
                    .FirstOrDefault();

                if (km != null)
                {
                    p.GiaGoc = p.GiaPhong;
                    p.GiaPhong = p.GiaPhong * (1 - (decimal)km.PhanTramGiam / 100);
                    p.KhuyenMaiHienTai = km;
                }

                return p;
            }).ToList();

            // 5. Tạo ViewModel
            var viewModel = new PhongIndexViewModel
            {
                DanhSachPhongs = danhSachPhongDaXuLy,
                PhanTrang = new PhanTrang(totalItems, page, pageSize),
                // ⭐ Gán lại tham số tìm kiếm để giữ trạng thái trong form ⭐
                NgayDen = finalNgayDen,
                NgayTra = finalNgayTra,
                SoNguoi = soNguoi,
                LoaiPhong = loaiPhong,
                DanhSachLoaiPhong = danhSachLoaiPhong
            };

            return View(viewModel);
        }

        [Authorize(Roles = "Admin,NhanVien")]
        [HttpGet]
        public IActionResult Them()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,NhanVien")]
        public async Task<IActionResult> Them(Phong phong, IFormFile HinhAnhFile)
        {
            if (ModelState.IsValid)
            {
                if (HinhAnhFile != null && HinhAnhFile.Length > 0)
                {
                    var fileName = Path.GetFileName(HinhAnhFile.FileName);
                    var path = Path.Combine(_env.WebRootPath, "HinhAnh", fileName);
                    using (var stream = new FileStream(path, FileMode.Create))
                    {
                        await HinhAnhFile.CopyToAsync(stream);
                    }
                    phong.HinhAnh = "/HinhAnh/" + fileName;
                }

                _context.Add(phong);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(phong);
        }

        [Authorize(Roles = "Admin,NhanVien")]
        public async Task<IActionResult> Sua(int id)
        {
            var phong = await _context.Phong.FindAsync(id);
            if (phong == null) return NotFound();
            return View(phong);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,NhanVien")]
        public async Task<IActionResult> Sua(int id, Phong phong, IFormFile? HinhAnhFile)
        {
            if (id != phong.MaPhong) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    if (HinhAnhFile != null && HinhAnhFile.Length > 0)
                    {
                        var fileName = Path.GetFileName(HinhAnhFile.FileName);
                        var path = Path.Combine(_env.WebRootPath, "HinhAnh", fileName);
                        using (var stream = new FileStream(path, FileMode.Create))
                        {
                            await HinhAnhFile.CopyToAsync(stream);
                        }
                        phong.HinhAnh = "/HinhAnh/" + fileName;
                    }

                    _context.Update(phong);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Phong.Any(e => e.MaPhong == id))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(phong);
        }

        [Authorize(Roles = "Admin,NhanVien")]
        public async Task<IActionResult> Xoa(int id)
        {
            var phong = await _context.Phong.FindAsync(id);
            if (phong == null) return NotFound();
            return View(phong);
        }

        [HttpPost, ActionName("Xoa")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,NhanVien")]
        public async Task<IActionResult> XoaConfirmed(int id)
        {
            var phong = await _context.Phong.FindAsync(id);
            if (phong != null)
            {
                _context.Phong.Remove(phong);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
        [AllowAnonymous]
        public async Task<IActionResult> ChiTiet(int id)
        {
            var phong = await _context.Phong
                .Include(p => p.PhongKhuyenMais)
                    .ThenInclude(pk => pk.KhuyenMai)
                .FirstOrDefaultAsync(p => p.MaPhong == id);

            if (phong == null)
                return NotFound();

            // ⭐ Lấy khuyến mãi cao nhất
            var km = phong.PhongKhuyenMais
                .Where(pk => pk.KhuyenMai.NgayBatDau <= DateTime.Now &&
                             pk.KhuyenMai.NgayKetThuc >= DateTime.Now)
                .OrderByDescending(pk => pk.KhuyenMai.PhanTramGiam)
                .Select(pk => pk.KhuyenMai)
                .FirstOrDefault();

            if (km != null)
            {
                phong.GiaGoc = phong.GiaPhong;
                phong.GiaPhong = phong.GiaPhong * (1 - (decimal)km.PhanTramGiam / 100);
                phong.KhuyenMaiHienTai = km;
            }

            // ⭐ Lấy đánh giá phòng
            var ratings = _context.DanhGiaPhongs
                .Where(d => d.MaPhong == phong.MaPhong)
                .ToList();

            phong.AverageStars = ratings.Any() ? ratings.Average(d => d.SoSao) : 0;
            phong.TotalReviews = ratings.Count;

            return View(phong);
        }


        [HttpGet]
        public IActionResult TimKiem(DateTime ngayDen, DateTime ngayTra, int soNguoi, string loaiPhong)
        {
            // ✅ Kiểm tra đã đăng nhập chưa
            var tenDangNhap = HttpContext.Session.GetString("TenDangNhap");
            var quyen = HttpContext.Session.GetString("Quyen");

            if (string.IsNullOrEmpty(tenDangNhap) || string.IsNullOrEmpty(quyen))
            {
                TempData["Error"] = "Vui lòng đăng nhập để sử dụng chức năng tìm kiếm phòng.";
                return RedirectToAction("DangNhap", "TaiKhoan");
            }

            // ✅ Lấy danh sách phòng còn trống
            var phongDaDat = _context.DatPhong
                .Where(dp => dp.NgayNhan < ngayTra && dp.NgayTra > ngayDen)
                .Select(dp => dp.MaPhong)
                .ToList();

            var query = _context.Phong
                .Where(p => !phongDaDat.Contains(p.MaPhong) && p.TrangThai == "ConTrong");

            if (!string.IsNullOrEmpty(loaiPhong))
            {
                query = query.Where(p => p.LoaiPhong == loaiPhong);
            }

            // Nếu bảng có cột Số người tối đa, có thể mở lại:
            // query = query.Where(p => p.SoNguoiToiDa >= soNguoi);

            var phongTrong = query.ToList();

            return View("KetQuaTimKiem", phongTrong);
        }



    }
}
