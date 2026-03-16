using DACN.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;

namespace DACN.Controllers
{
    public class PhieuHuHaiController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;

        public PhieuHuHaiController(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // KHÁCH HÀNG GỬI PHIẾU
        [HttpGet]
        public IActionResult Create()
        {
            ViewBag.PhongList = _context.Phong.ToList();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(PhieuHuHai phieu, IFormFile? AnhMinhChung)
        {
            if (AnhMinhChung != null)
            {
                var uploadDir = Path.Combine(_env.WebRootPath, "uploads/hu_hai");
                if (!Directory.Exists(uploadDir))
                    Directory.CreateDirectory(uploadDir);

                var fileName = Guid.NewGuid() + Path.GetExtension(AnhMinhChung.FileName);
                var filePath = Path.Combine(uploadDir, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await AnhMinhChung.CopyToAsync(stream);
                }

                phieu.AnhMinhChung = "/uploads/hu_hai/" + fileName;
            }

            phieu.ThoiGianGhiNhan = DateTime.Now;
            phieu.TrangThai = "Chờ xử lý";

            _context.PhieuHuHai.Add(phieu);

            // Gửi thông báo cho nhân viên
            _context.ThongBao.Add(new ThongBao
            {
                NoiDung = $"Có phiếu hư hại mới từ phòng {phieu.MaPhong}.",
                ThoiGian = DateTime.Now,
                DaDoc = false,
                NguoiNhan = "NhanVien" // hoặc mã nhân viên thực tế
            });

            await _context.SaveChangesAsync();
            TempData["Success"] = "Báo cáo hư hại đã được gửi!";
            return RedirectToAction("Create");
        }

        // NHÂN VIÊN XEM DANH SÁCH
        public async Task<IActionResult> Index()
        {
            var list = await _context.PhieuHuHai
                .Include(p => p.Phong)
                .OrderByDescending(p => p.ThoiGianGhiNhan)
                .ToListAsync();

            return View(list);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var phieu = await _context.PhieuHuHai.FindAsync(id);
            if (phieu == null) return NotFound();
            return View(phieu);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(PhieuHuHai phieu)
        {
            var existing = await _context.PhieuHuHai.FindAsync(phieu.MaPhieu);
            if (existing == null) return NotFound();

            existing.TrangThai = phieu.TrangThai;
            existing.ChiPhiBoiThuong = phieu.ChiPhiBoiThuong;
            existing.NguyenNhan = phieu.NguyenNhan;

            await _context.SaveChangesAsync();
            TempData["Success"] = "Cập nhật phiếu hư hại thành công!";
            return RedirectToAction("Index");
        }

        // ADMIN QUẢN LÝ
        public async Task<IActionResult> AdminList()
        {
            var list = await _context.PhieuHuHai
                .Include(p => p.Phong)
                .OrderByDescending(p => p.ThoiGianGhiNhan)
                .ToListAsync();

            return View(list);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var phieu = await _context.PhieuHuHai
                .Include(p => p.Phong)
                .FirstOrDefaultAsync(p => p.MaPhieu == id);
            if (phieu == null) return NotFound();
            return View(phieu);
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var phieu = await _context.PhieuHuHai.FindAsync(id);
            if (phieu == null) return NotFound();
            return View(phieu);
        }

        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var phieu = await _context.PhieuHuHai.FindAsync(id);
            if (phieu != null)
            {
                _context.PhieuHuHai.Remove(phieu);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("AdminList");
        }
    }
}
