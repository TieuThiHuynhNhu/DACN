using DACN.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System; // Cần thiết cho DateTime.Now
using System.Threading.Tasks;
using System.Linq;
using System.IO; // Cần thiết cho Path, Directory, FileStream
using Microsoft.AspNetCore.Http; // Cần thiết cho IFormFile

namespace DACN.Controllers
{
    // Đặt Authorize cho toàn bộ Controller (mọi người đều có thể truy cập)
    // Nhưng sau đó sẽ phân quyền chi tiết cho từng Action
    [Authorize(Roles = "Admin,KhachHang,NhanVien")]
    public class KhuyenMaiController : Controller
    {
        private readonly ApplicationDbContext _context;

        public KhuyenMaiController(ApplicationDbContext context)
        {
            _context = context;
        }

        // =================================================================
        // 🚀 1. PHẦN DÀNH CHO KHÁCH HÀNG (User/Public)
        // Giao diện hiển thị danh sách khuyến mãi hấp dẫn (Card View)
        // =================================================================

        // [Không cần Authorize] Khuyến mãi thường là công khai.
        // Tuy nhiên, nếu bạn muốn chỉ Khách Hàng đã đăng nhập mới xem được, giữ nguyên [Authorize]
        public async Task<IActionResult> DanhSach()
        {
            // Chỉ lấy các khuyến mãi ĐANG DIỄN RA
            var dsKhuyenMai = await _context.KhuyenMai
                .Where(k => k.NgayBatDau <= DateTime.Now && k.NgayKetThuc >= DateTime.Now)
                .OrderByDescending(k => k.NgayBatDau)
                .ToListAsync();

            // Sẽ cần một View mới (Ví dụ: Views/KhuyenMai/DanhSach.cshtml) để hiển thị Card View hấp dẫn.
            return View(dsKhuyenMai);
        }

        // Action xem chi tiết khuyến mãi (Ví dụ: để xem Điều khoản & Điều kiện)
        public async Task<IActionResult> ChiTiet(int id)
        {
            var km = await _context.KhuyenMai
                .Include(k => k.PhongKhuyenMais)
                    .ThenInclude(pk => pk.Phong)
                .FirstOrDefaultAsync(k => k.MaKhuyenMai == id);

            if (km == null)
                return NotFound();

            return View(km); // View ChiTiet.cshtml sẽ là giao diện công khai
        }

        // =================================================================
        // 💼 2. PHẦN DÀNH CHO NHÂN VIÊN (Staff/Operational)
        // Giao diện tra cứu nhanh (Dạng Bảng đơn giản)
        // =================================================================

        [Authorize(Roles = "Admin,NhanVien")]
        public async Task<IActionResult> TraCuu()
        {
            // Nhân viên có thể cần xem tất cả (Đang diễn ra, sắp, đã hết) để hỗ trợ khách
            var ds = await _context.KhuyenMai
                .Include(k => k.PhongKhuyenMais)
                    .ThenInclude(pk => pk.Phong)
                .OrderByDescending(k => k.NgayBatDau)
                .ToListAsync();

            // View TraCuu.cshtml (Dạng bảng tra cứu, không có nút Sửa/Xóa)
            return View(ds);
        }


        // =================================================================
        // 👑 3. PHẦN DÀNH CHO ADMIN (Management/CRUD)
        // Giao diện quản lý (Dạng Bảng đầy đủ với Sửa/Xóa)
        // =================================================================

        // Đổi tên Index thành QuanLy và chỉ cho phép Admin truy cập
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index()
        {
            var ds = await _context.KhuyenMai
                .Include(k => k.PhongKhuyenMais)
                    .ThenInclude(pk => pk.Phong)
                .OrderByDescending(k => k.MaKhuyenMai)
                .ToListAsync();

            // View QuanLy.cshtml (Dạng bảng cho Admin, có đủ nút Sửa, Xóa)
            return View(ds);
        }

        // Các Action CRUD (Thêm, Sửa, Xóa) chỉ dành cho Admin
        [Authorize(Roles = "Admin")]
        public IActionResult Them()
        {
            ViewBag.Phongs = _context.Phong.ToList();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public IActionResult Them(KhuyenMai model, List<int> PhongIds, IFormFile? HinhAnhFile)
        {
            // Giữ nguyên logic xử lý Thêm của bạn
            if (!PhongIds.Any())
            {
                ModelState.AddModelError("", "Vui lòng chọn ít nhất một phòng.");
                ViewBag.Phongs = _context.Phong.ToList();
                return View(model);
            }

            // Xử lý hình ảnh (Giữ nguyên logic của bạn)
            string? filePath = null;
            if (HinhAnhFile != null && HinhAnhFile.Length > 0)
            {
                // Thêm logic đảm bảo thư mục tồn tại
                var wwwrootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                var imagePath = Path.Combine(wwwrootPath, "HinhAnh");

                if (!Directory.Exists(imagePath))
                {
                    Directory.CreateDirectory(imagePath);
                }

                var fileName = Path.GetFileName(HinhAnhFile.FileName);
                var savePath = Path.Combine(imagePath, fileName);
                using var stream = new FileStream(savePath, FileMode.Create);
                HinhAnhFile.CopyTo(stream);
                filePath = "/HinhAnh/" + fileName;
            }
            model.HinhAnh = filePath;

            // Đảm bảo collection được khởi tạo
            if (model.PhongKhuyenMais == null)
            {
                model.PhongKhuyenMais = new List<PhongKhuyenMai>();
            }

            // Thêm các phòng
            foreach (var maPhong in PhongIds)
            {
                model.PhongKhuyenMais.Add(new PhongKhuyenMai
                {
                    MaPhong = maPhong
                });
            }

            _context.KhuyenMai.Add(model);
            _context.SaveChanges();

            return RedirectToAction(nameof(Index)); // Chuyển hướng về trang Quản lý
        }


        // Form sửa
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Sua(int id)
        {
            // Sửa logic FindAsync thành FirstOrDefaultAsync để đảm bảo Include được load
            var km = await _context.KhuyenMai
                                   .Include(k => k.PhongKhuyenMais)
                                   .FirstOrDefaultAsync(k => k.MaKhuyenMai == id);

            if (km == null)
                return NotFound();

            ViewBag.DanhSachPhong = _context.Phong.ToList();
            return View(km);
        }

        // Xử lý sửa
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public IActionResult Sua(KhuyenMai model, IFormFile? HinhAnhFile, int[]? PhongIds)
        {
            // Giữ nguyên logic xử lý Sửa của bạn
            var km = _context.KhuyenMai
                             .Include(k => k.PhongKhuyenMais)
                             .FirstOrDefault(k => k.MaKhuyenMai == model.MaKhuyenMai);

            if (km == null) return NotFound();

            // Cập nhật thông tin cơ bản
            km.TenKhuyenMai = model.TenKhuyenMai;
            km.PhanTramGiam = model.PhanTramGiam;
            km.MoTa = model.MoTa;
            km.NgayBatDau = model.NgayBatDau;
            km.NgayKetThuc = model.NgayKetThuc;

            // Hình ảnh
            if (HinhAnhFile != null && HinhAnhFile.Length > 0)
            {
                var wwwrootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                var imagePath = Path.Combine(wwwrootPath, "HinhAnh");

                if (!Directory.Exists(imagePath))
                {
                    Directory.CreateDirectory(imagePath);
                }

                var fileName = Path.GetFileName(HinhAnhFile.FileName);
                var savePath = Path.Combine(imagePath, fileName);
                using var stream = new FileStream(savePath, FileMode.Create);
                HinhAnhFile.CopyTo(stream);
                km.HinhAnh = "/HinhAnh/" + fileName;
            }

            // --- Cập nhật các phòng ---
            // Xóa tất cả và thêm lại là cách đơn giản nhất cho M-N
            km.PhongKhuyenMais.Clear();
            if (PhongIds != null && PhongIds.Any())
            {
                foreach (var maPhong in PhongIds)
                {
                    km.PhongKhuyenMais.Add(new PhongKhuyenMai { MaKhuyenMai = km.MaKhuyenMai, MaPhong = maPhong });
                }
            }

            _context.SaveChanges();
            return RedirectToAction(nameof(Index)); // Chuyển hướng về trang Quản lý
        }


        // Xác nhận xoá
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Xoa(int id)
        {
            var km = await _context.KhuyenMai
                                    .Include(k => k.PhongKhuyenMais) // Thay vì include p.Phong
                                    .FirstOrDefaultAsync(k => k.MaKhuyenMai == id);

            if (km == null)
            {
                return NotFound();
            }
            // Load thông tin phòng để hiển thị trên View xác nhận Xóa (tùy chọn)
            ViewBag.TenPhongs = km.PhongKhuyenMais
                                  .Select(pk => _context.Phong.Find(pk.MaPhong)?.LoaiPhong)
                                  .Where(t => t != null)
                                  .ToList();
            return View(km);
        }

        // Xử lý Xóa
        [HttpPost, ActionName("XoaConfirmed")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public IActionResult XoaConfirmed(int MaKhuyenMai)
        {
            var km = _context.KhuyenMai
                             .Include(k => k.PhongKhuyenMais) // Cần include để xóa các bản ghi liên quan
                             .FirstOrDefault(k => k.MaKhuyenMai == MaKhuyenMai);

            if (km == null)
            {
                return NotFound();
            }

            // Nếu sử dụng cascade delete trong DB thì không cần Clear/Remove
            // Nếu không, cần xóa thủ công các bản ghi trong bảng trung gian:
            _context.PhongKhuyenMai.RemoveRange(km.PhongKhuyenMais);

            _context.KhuyenMai.Remove(km);
            _context.SaveChanges();

            return RedirectToAction(nameof(Index)); // Chuyển hướng về trang Quản lý
        }
    }
}