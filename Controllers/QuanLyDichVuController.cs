using DACN.Models;
using DACN.Models.ViewModel;
using iTextSharp.text.pdf;
using iTextSharp.text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace DACN.Controllers
{
    [Authorize(Roles = "Admin,NhanVien")]
    public class QuanLyDichVuController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;

        public QuanLyDichVuController(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // ---------------------------
        // Danh sách dịch vụ (Không thay đổi)
        // ---------------------------
        public IActionResult Index(int page = 1, string searchDichVu = "")
        {
            int pageSize = 6;

            var dichVuQuery = _context.DichVu.AsQueryable();

            if (!string.IsNullOrEmpty(searchDichVu))
            {
                string searchDichVuLower = searchDichVu.ToLower().Trim();
                dichVuQuery = dichVuQuery.Where(d =>
                    d.TenDichVu.ToLower().Trim().Contains(searchDichVuLower));

                ViewData["CurrentFilterDichVu"] = searchDichVu;
            }

            var dichVus = dichVuQuery.OrderByDescending(d => d.MaDichVu).ToList();
            int totalItems = dichVus.Count;

            var phanTrang = new PhanTrang(totalItems, page, pageSize);

            var model = new DichVuIndexViewModel
            {
                DanhSachDichVus = dichVus.Skip((page - 1) * pageSize).Take(pageSize).ToList(),
                PhanTrang = phanTrang
            };

            return View(model);
        }

        // ---------------------------
        // Thêm dịch vụ (Không thay đổi)
        // ---------------------------
        [HttpGet]
        public IActionResult Them() => View();

        [HttpPost]
        public IActionResult Them(DichVu model, IFormFile HinhAnhFile)
        {
            if (!ModelState.IsValid) return View(model);

            if (HinhAnhFile != null && HinhAnhFile.Length > 0)
            {
                string fileName = Guid.NewGuid() + Path.GetExtension(HinhAnhFile.FileName);
                string uploadPath = Path.Combine(_env.WebRootPath, "HinhAnh");
                if (!Directory.Exists(uploadPath))
                    Directory.CreateDirectory(uploadPath);

                string filePath = Path.Combine(uploadPath, fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    HinhAnhFile.CopyTo(stream);
                }

                model.HinhAnh = "/HinhAnh/" + fileName;
            }

            model.TrangThai = "ConHoatDong";
            _context.DichVu.Add(model);
            _context.SaveChanges();

            TempData["Success"] = "Thêm dịch vụ thành công!";
            return RedirectToAction("Index");
        }

        // ---------------------------
        // Sửa dịch vụ (Không thay đổi)
        // ---------------------------
        [HttpGet]
        public IActionResult Sua(int id)
        {
            var dv = _context.DichVu.FirstOrDefault(d => d.MaDichVu == id);
            if (dv == null) return NotFound();
            return View(dv);
        }

        [HttpPost]
        public IActionResult Sua(DichVu model, IFormFile HinhAnhFile)
        {
            if (!ModelState.IsValid) return View(model);

            var dv = _context.DichVu.FirstOrDefault(d => d.MaDichVu == model.MaDichVu);
            if (dv == null) return NotFound();

            dv.TenDichVu = model.TenDichVu;
            dv.MoTa = model.MoTa;
            dv.GiaDichVu = model.GiaDichVu;
            dv.MucLuc = model.MucLuc;
            dv.TrangThai = model.TrangThai;

            if (HinhAnhFile != null && HinhAnhFile.Length > 0)
            {
                string fileName = Guid.NewGuid() + Path.GetExtension(HinhAnhFile.FileName);
                string uploadPath = Path.Combine(_env.WebRootPath, "HinhAnh");
                if (!Directory.Exists(uploadPath))
                    Directory.CreateDirectory(uploadPath);

                string filePath = Path.Combine(uploadPath, fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    HinhAnhFile.CopyTo(stream);
                }

                dv.HinhAnh = "/HinhAnh/" + fileName;
            }

            _context.SaveChanges();
            TempData["Success"] = "Cập nhật dịch vụ thành công!";
            return RedirectToAction("Index");
        }

        // ---------------------------
        // Ngừng cung cấp dịch vụ (Không thay đổi)
        // ---------------------------
        [HttpGet]
        public IActionResult NgungCungCap(int id)
        {
            var dv = _context.DichVu.FirstOrDefault(d => d.MaDichVu == id);
            if (dv == null) return NotFound();

            dv.TrangThai = "NgungCungCap";
            _context.SaveChanges();

            TempData["Success"] = "Ngừng cung cấp dịch vụ thành công!";
            return RedirectToAction("Index");
        }

        // ---------------------------
        // Xóa vĩnh viễn dịch vụ (Không thay đổi)
        // ---------------------------
        [HttpGet]
        // ⭐ Đảm bảo chỉ Admin mới được xóa vĩnh viễn ⭐
        [Authorize(Roles = "Admin")]
        public IActionResult XoaVinhVien(int id)
        {
            var dv = _context.DichVu.FirstOrDefault(d => d.MaDichVu == id);
            if (dv == null) return NotFound();

            if (!string.IsNullOrEmpty(dv.HinhAnh))
            {
                string filePath = Path.Combine(_env.WebRootPath, dv.HinhAnh.TrimStart('/').Replace("/", "\\"));
                if (System.IO.File.Exists(filePath))
                    System.IO.File.Delete(filePath);
            }

            _context.DichVu.Remove(dv);
            _context.SaveChanges();

            TempData["Success"] = "Xóa dịch vụ vĩnh viễn thành công!";
            return RedirectToAction("Index");
        }

        // =============================================================
        // ⭐ CẬP NHẬT LICH SU DAT DICH VU (Sửa lỗi Model và Logic lọc) ⭐
        // =============================================================
        // --- 1. Xem lịch sử tổng quát (Dành cho Admin/Báo cáo) ---
        public IActionResult LichSuDatDichVu(string tenKhachHang = "", string thangNam = "")
        {
            var lichSuQuery = _context.DatDichVu
                .Include(d => d.DichVu).Include(d => d.KhachHang).AsQueryable();

            if (!string.IsNullOrEmpty(tenKhachHang))
                lichSuQuery = lichSuQuery.Where(d => d.KhachHang.HoTen.Contains(tenKhachHang));

            var lichSu = lichSuQuery.OrderByDescending(d => d.MaDatDichVu).ToList();
            return View(new LichSuDatDichVuViewModel { LichSuDichVu = lichSu, TenKhachHang = tenKhachHang, ThangNam = thangNam });
        }

        // --- 2. TRẠM TIẾP NHẬN DỊCH VỤ (Dành riêng cho Nhân viên xử lý) ---
        [HttpGet]
        public async Task<IActionResult> TiepNhanDichVu()
        {
            // Bước 1: Lọc dữ liệu thô từ Database (Loại bỏ các đơn đã xong/hủy)
            // Chúng ta chỉ OrderBy theo NgaySuDung (kiểu DateTime - SQLite hỗ trợ tốt)
            var requestsInDb = await _context.DatDichVu
                .Include(d => d.DichVu)
                .Include(d => d.KhachHang)
                .Where(d => d.TrangThai != "HoanTat" && d.TrangThai != "DaHuy")
                .OrderByDescending(d => d.NgaySuDung)
                .ToListAsync();

            // Bước 2: Sắp xếp theo KhungGio (TimeSpan) trên Client (LINQ to Objects)
            // Điều này giúp tránh lỗi NotSupportedException của SQLite
            var requestsSorted = requestsInDb
                .OrderByDescending(d => d.NgaySuDung)
                .ThenBy(d => d.KhungGio)
                .ToList();

            return View(requestsSorted);
        }

        // --- 3. Action cập nhật trạng thái nhanh ---
        [HttpPost]
        public async Task<IActionResult> CapNhatTienDo(int id, string trangThaiMoi)
        {
            var datDichVu = await _context.DatDichVu.FindAsync(id);
            if (datDichVu == null) return NotFound();

            datDichVu.TrangThai = trangThaiMoi;
            _context.Update(datDichVu);
            await _context.SaveChangesAsync();

            string message = trangThaiMoi switch
            {
                "DangXuLy" => "Đã tiếp nhận yêu cầu và bắt đầu chuẩn bị.",
                "HoanTat" => "Dịch vụ đã hoàn tất phục vụ.",
                "DaHuy" => "Đã hủy lịch hẹn dịch vụ.",
                _ => "Đã cập nhật trạng thái."
            };

            TempData["Success"] = message;
            return RedirectToAction(nameof(TiepNhanDichVu));
        }
        // =========================================================
        // ⭐ CẬP NHẬT XUAT PDF (Sửa logic lọc theo Khách hàng/Tháng) ⭐
        // =========================================================
        [HttpGet]
        [Authorize(Roles = "Admin,NhanVien")]
        public IActionResult XuatPdf(string tenKhachHang, string thangNam)
        {
            var lichSuQuery = _context.DatDichVu
                .Include(d => d.KhachHang)
                .Include(d => d.DichVu)
                .AsQueryable();

            // Lọc theo Tên khách hàng (Giống logic trong LichSuDatDichVu)
            if (!string.IsNullOrEmpty(tenKhachHang))
            {
                string searchName = tenKhachHang.ToLower().Trim();
                lichSuQuery = lichSuQuery.Where(d =>
                    d.KhachHang != null && d.KhachHang.HoTen.ToLower().Contains(searchName));
            }

            // Lọc theo Tháng/Năm (mm/yy)
            if (!string.IsNullOrEmpty(thangNam) && DateTime.TryParseExact(thangNam, "MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsedMonthYear))
            {
                int month = parsedMonthYear.Month;
                int year = parsedMonthYear.Year;

                lichSuQuery = lichSuQuery.Where(d =>
                    d.NgaySuDung.HasValue && d.NgaySuDung.Value.Month == month && d.NgaySuDung.Value.Year == year);
            }

            var datas = lichSuQuery.OrderByDescending(d => d.NgaySuDung).ToList();

            using (MemoryStream ms = new MemoryStream())
            {
                // Cấu hình PDF như cũ...
                Document doc = new Document(PageSize.A4, 25, 25, 30, 30);
                PdfWriter.GetInstance(doc, ms);
                doc.Open();

                // Sử dụng font hỗ trợ Unicode (Giả định tệp tin 'arial.ttf' tồn tại)
                string fontPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Fonts), "arial.ttf");
                BaseFont bf = BaseFont.CreateFont(fontPath, BaseFont.IDENTITY_H, BaseFont.EMBEDDED);
                Font font = new Font(bf, 12);

                // Tiêu đề
                Paragraph title = new Paragraph("LỊCH SỬ ĐẶT DỊCH VỤ - SEAHOTEL", new Font(bf, 16, Font.BOLD));
                title.Alignment = Element.ALIGN_CENTER;
                doc.Add(title);

                // Thêm thông tin lọc
                string filterInfo = "Thời gian: " + (string.IsNullOrEmpty(thangNam) ? "Tất cả" : thangNam);
                filterInfo += " | Khách hàng: " + (string.IsNullOrEmpty(tenKhachHang) ? "Tất cả" : tenKhachHang);
                doc.Add(new Paragraph(filterInfo, new Font(bf, 10)));
                doc.Add(new Paragraph("\n"));

                // Bảng
                PdfPTable table = new PdfPTable(8);
                table.WidthPercentage = 100;
                table.SetWidths(new float[] { 5, 15, 20, 12, 10, 8, 10, 10 });

                // Header
                string[] headers = { "#", "Khách hàng", "Dịch vụ", "Ngày sử dụng", "Khung giờ", "Số lượng", "Thành tiền", "Trạng thái" };
                foreach (var h in headers)
                {
                    PdfPCell cell = new PdfPCell(new Phrase(h, font))
                    {
                        BackgroundColor = BaseColor.LIGHT_GRAY,
                        HorizontalAlignment = Element.ALIGN_CENTER
                    };
                    table.AddCell(cell);
                }

                // Dữ liệu
                int stt = 1;
                foreach (var item in datas)
                {
                    table.AddCell(new PdfPCell(new Phrase(stt.ToString(), font)));
                    table.AddCell(new PdfPCell(new Phrase(item.KhachHang?.HoTen ?? $"#{item.MaKhachHang}", font)));
                    table.AddCell(new PdfPCell(new Phrase(item.DichVu?.TenDichVu ?? $"#{item.MaDichVu}", font)));
                    table.AddCell(new PdfPCell(new Phrase(item.NgaySuDung?.ToString("dd/MM/yyyy") ?? "-", font)));
                    table.AddCell(new PdfPCell(new Phrase(item.KhungGio?.ToString(@"hh\:mm") ?? "-", font)));
                    table.AddCell(new PdfPCell(new Phrase(item.SoLuong.ToString(), font)));
                    table.AddCell(new PdfPCell(new Phrase(item.ThanhTien.ToString("N0") + " VNĐ", font)));
                    table.AddCell(new PdfPCell(new Phrase(item.TrangThai, font)));
                    stt++;
                }

                doc.Add(table);
                doc.Close();

                return File(ms.ToArray(), "application/pdf", "LichSuDatDichVu.pdf");
            }
        }
    }
}