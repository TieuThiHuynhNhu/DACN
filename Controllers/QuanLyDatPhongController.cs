using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DACN.Models;
using System.Linq;
using System.Threading.Tasks;

namespace DACN.Controllers
{
    public class QuanLyDatPhongController : Controller
    {
        private readonly ApplicationDbContext _context;

        public QuanLyDatPhongController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ⭐ Action chính cho hiển thị danh sách và tìm kiếm ⭐
        public IActionResult Index(string searchLoaiPhong, string searchKhachHang)
        {
            return TimKiem(searchLoaiPhong, searchKhachHang, null);
        }

        // Chỉ phòng đã đặt hoặc đã xác nhận (có hỗ trợ tìm kiếm)
        public IActionResult PhongDaDat(string searchLoaiPhong, string searchKhachHang)
        {
            // Bao gồm các trạng thái mà phòng đang được sử dụng hoặc đã được giữ
            return TimKiem(searchLoaiPhong, searchKhachHang, new[] { "DaDat", "DangSuDung", "ChoThanhToan" });
        }

        // Chỉ phòng còn trống (có hỗ trợ tìm kiếm)
        public IActionResult PhongConTrong(string searchLoaiPhong, string searchKhachHang)
        {
            return TimKiem(searchLoaiPhong, searchKhachHang, new[] { "ConTrong" });
        }

        // ⭐ ACTION HELPER MỚI: Xử lý logic tìm kiếm và lọc trạng thái TỐI ƯU ⭐
        // Trong DACN.Controllers.QuanLyDatPhongController.cs

        // Trong DACN.Controllers.QuanLyDatPhongController.cs

        // Trong DACN.Controllers.QuanLyDatPhongController.cs

        // Trong DACN.Controllers.QuanLyDatPhongController.cs

        // Trong DACN.Controllers.QuanLyDatPhongController.cs

        // Trong DACN.Controllers.QuanLyDatPhongController.cs

        // Trong DACN.Controllers.QuanLyDatPhongController.cs

        private IActionResult TimKiem(string searchLoaiPhong, string searchKhachHang, string[] allowedStatuses)
        {
            // Bắt đầu truy vấn PHÒNG (Toàn bộ danh sách)
            var phongQuery = _context.Phong.AsQueryable();

            // --- Chuẩn hóa dữ liệu tìm kiếm (Bước 1: Áp dụng Trim() cho đầu vào) ---
            // Loại bỏ khoảng trắng thừa ở chuỗi tìm kiếm đầu vào
            string searchLoaiPhongLower = string.IsNullOrEmpty(searchLoaiPhong) ? null : searchLoaiPhong.ToLower().Trim();
            string searchKhachHangLower = string.IsNullOrEmpty(searchKhachHang) ? null : searchKhachHang.ToLower().Trim();

            // 1. Lọc TẤT CẢ phòng theo Loại phòng (Không phân biệt trạng thái)
            if (searchLoaiPhongLower != null)
            {
                // ⭐ QUAN TRỌNG: Áp dụng .Trim() cho DB value (p.LoaiPhong) để khắc phục lỗi khoảng trắng trong CSDL ⭐
                phongQuery = phongQuery.Where(p => p.LoaiPhong.ToLower().Trim().Contains(searchLoaiPhongLower));
            }

            // 2. Lọc theo Trạng thái (từ các tab: Tất cả, Đã đặt, Còn trống, Chờ xác nhận)
            // Lọc này giới hạn kết quả theo tab mà người dùng đang chọn.
            if (allowedStatuses != null && allowedStatuses.Length > 0)
            {
                phongQuery = phongQuery.Where(p => allowedStatuses.Contains(p.TrangThai));
            }

            // 3. Lọc thêm theo Khách hàng (CHỈ áp dụng cho các trạng thái có Đặt phòng)
            // Bao gồm tất cả các trạng thái có liên quan đến khách hàng để tìm kiếm.
            bool isSearchingActiveRooms = allowedStatuses == null ||
                                          allowedStatuses.Any(s => s == "DaDat" || s == "DangSuDung" || s == "ChoThanhToan" || s == "ChoXacNhan");

            if (searchKhachHangLower != null && isSearchingActiveRooms)
            {
                // Lấy Mã phòng của các đặt phòng đang hoạt động khớp với tên khách hàng
                var maPhongByKhachHang = _context.DatPhong
                    .Include(d => d.KhachHang)
                    .Where(d =>
                        // Lọc các trạng thái đặt phòng chứa thông tin khách hàng
                        (d.TrangThai == "DaDat" || d.TrangThai == "DangSuDung" || d.TrangThai == "ChoThanhToan" || d.TrangThai == "ChoXacNhan") &&
                        d.KhachHang != null &&
                        // Áp dụng .Trim() cho tên khách hàng
                        d.KhachHang.HoTen.ToLower().Trim().Contains(searchKhachHangLower)
                    )
                    .Select(d => d.MaPhong);

                // Chỉ giữ lại các phòng có mã khớp với kết quả lọc khách hàng
                phongQuery = phongQuery.Where(p => maPhongByKhachHang.Contains(p.MaPhong));
            }

            // --- KẾT THÚC XỬ LÝ LỌC ---

            // Thực thi truy vấn và lấy danh sách phòng
            var phongList = phongQuery.ToList();

            // Truyền từ khóa tìm kiếm để giữ lại trên ô tìm kiếm
            ViewData["CurrentFilterLoaiPhong"] = searchLoaiPhong;
            ViewData["CurrentFilterKhachHang"] = searchKhachHang;

            return View("Index", phongList);
        }
        // ⭐ ACTION API: Trả về danh sách Loại phòng duy nhất (cho Autocomplete) ⭐
        [HttpGet]
        public IActionResult GetLoaiPhongSuggest(string term)
        {
            var loaiPhongs = _context.Phong
                .Select(p => p.LoaiPhong)
                .Distinct()
                .Where(lp => lp.ToLower().Contains(term.ToLower()))
                .Take(10)
                .ToList();

            // Trả về dưới dạng JSON
            return Json(loaiPhongs);
        }

        // Chi tiết phòng + khách đặt
        public IActionResult ChiTiet(int maPhong)
        {
            var phong = _context.Phong.FirstOrDefault(p => p.MaPhong == maPhong);
            if (phong == null) return NotFound();

            // Lấy Đặt phòng có trạng thái đang active
            var datPhong = _context.DatPhong
                .Include(d => d.KhachHang)
                .FirstOrDefault(d => d.MaPhong == maPhong && (d.TrangThai == "DaDat" || d.TrangThai == "ChoThanhToan" || d.TrangThai == "DangSuDung"));

            ViewBag.DatPhong = datPhong;

            return View(phong);
        }

        [HttpPost]
        public IActionResult CapNhatTrangThaiPhong(int maPhong, string trangThai)
        {
            var phong = _context.Phong.FirstOrDefault(p => p.MaPhong == maPhong);
            if (phong == null) return NotFound();

            // 1. Cập nhật trạng thái PHÒNG
            phong.TrangThai = trangThai;

            // 2. Lấy Đặt phòng đang active
            var datPhong = _context.DatPhong
                .FirstOrDefault(d => d.MaPhong == maPhong && d.TrangThai != "HoanTat" && d.TrangThai != "DaHuy");

            if (datPhong != null)
            {
                if (trangThai == "ConTrong")
                {
                    // Nếu NV cập nhật phòng thành trống, coi đặt phòng này đã hoàn tất.
                    datPhong.TrangThai = "HoanTat";
                }
                else if (trangThai == "DaDat" || trangThai == "DangSuDung")
                {
                    // Cập nhật trạng thái đặt phòng tương ứng nếu đang chờ
                    if (datPhong.TrangThai == "ChoXacNhan" || datPhong.TrangThai == "ChoThanhToan")
                    {
                        datPhong.TrangThai = "DaDat";
                    }
                }
            }

            _context.SaveChanges();

            return RedirectToAction("ChiTiet", new { maPhong = maPhong });
        }

        // Hiển thị danh sách chờ xác nhận
        public async Task<IActionResult> ChoXacNhan()
        {
            var datPhongs = await _context.DatPhong
                .Include(d => d.KhachHang)
                .Include(d => d.Phong)
                .Where(d => d.TrangThai == "ChoXacNhan")
                .ToListAsync();

            return View(datPhongs);
        }

        // Nhân viên xác nhận đặt phòng
        [HttpPost]
        public async Task<IActionResult> XacNhan(int id)
        {
            var datPhong = await _context.DatPhong
                .Include(d => d.Phong)
                .FirstOrDefaultAsync(d => d.MaDatPhong == id);

            if (datPhong != null && datPhong.TrangThai == "ChoXacNhan")
            {
                datPhong.TrangThai = "DaDat";
                datPhong.Phong.TrangThai = "DaDat"; // cập nhật trạng thái phòng
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(ChoXacNhan));
        }

        [HttpPost]
        public async Task<IActionResult> Xoa(int id)
        {
            var datPhong = await _context.DatPhong
                .Include(d => d.Phong)
                .FirstOrDefaultAsync(d => d.MaDatPhong == id);

            if (datPhong == null)
                return NotFound();

            // Cập nhật trạng thái phòng
            if (datPhong.Phong != null)
            {
                datPhong.Phong.TrangThai = "ConTrong";
            }

            _context.DatPhong.Remove(datPhong);
            await _context.SaveChangesAsync();

            TempData["Message"] = "Đã xóa đặt phòng thành công!";
            return RedirectToAction(nameof(ChoXacNhan));
        }

        // Nhân viên xác nhận khách đã thanh toán, hoàn tất giao dịch và reset phòng
        [HttpPost]
        public async Task<IActionResult> TraPhongThanhCong(int maDatPhong)
        {
            var datPhong = await _context.DatPhong
                .Include(dp => dp.Phong)
                .FirstOrDefaultAsync(dp => dp.MaDatPhong == maDatPhong &&
                                         (dp.TrangThai == "DaDat" || dp.TrangThai == "ChoThanhToan" || dp.TrangThai == "DangSuDung"));

            if (datPhong == null)
            {
                TempData["Error"] = "Không tìm thấy đặt phòng hoặc trạng thái không hợp lệ.";
                return RedirectToAction(nameof(PhongDaDat));
            }

            // 1. Cập nhật trạng thái Đặt phòng thành "HoanTat"
            datPhong.TrangThai = "HoanTat";

            // 2. Cập nhật trạng thái Phòng thành "ConTrong"
            if (datPhong.Phong != null)
            {
                datPhong.Phong.TrangThai = "ConTrong";
                _context.Phong.Update(datPhong.Phong);
            }

            // 3. Lưu thay đổi
            await _context.SaveChangesAsync();

            TempData["Message"] = $"Đã hoàn tất thanh toán và trả phòng cho Mã đặt phòng: {maDatPhong}. Phòng đã được reset thành Trống.";

            return RedirectToAction(nameof(PhongDaDat));
        }
    }
}