using DACN.Models;
using iTextSharp.text.pdf;
using iTextSharp.text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DACN.Controllers
{
    [Authorize(Roles = "Admin,NhanVien")]
    public class ChamCongController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;

        public ChamCongController(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // ✅ Trang mặc định: Chấm công
        // ✅ Nhân viên vào Index để chấm công
        [Authorize(Roles = "NhanVien")]
        public IActionResult Index()
        {
            return View(); // view này dành cho nhân viên tự chấm công
        }

        // ✅ Admin có riêng trang để xem toàn bộ lịch sử chấm công
        [Authorize(Roles = "Admin")]
        public IActionResult IndexAdmin()
        {
            var lichSu = _context.ChamCong
                .Include(c => c.NhanVien) // load thông tin nhân viên
                .OrderByDescending(c => c.NgayGioVaoCa)
                .ToList();

            return View(lichSu); // view riêng cho admin
        }


        // Nhân viên Check-in
        [HttpPost]
        public IActionResult CheckIn(string anhKhuonMatBase64)
        {
            var username = HttpContext.Session.GetString("TenDangNhap");
            if (string.IsNullOrEmpty(username))
            {
                TempData["Error"] = "Bạn chưa đăng nhập!";
                return RedirectToAction("DangNhap", "TaiKhoan");
            }

            // 🔍 Tìm nhân viên theo username
            var nhanVien = _context.NhanVien.FirstOrDefault(nv => nv.TaiKhoanDangNhap == username);
            if (nhanVien == null)
            {
                TempData["Error"] = "Tài khoản này chưa được gán cho nhân viên nào!";
                return RedirectToAction("Index");
            }

            if (string.IsNullOrEmpty(anhKhuonMatBase64))
            {
                TempData["Error"] = "Chưa chụp được ảnh!";
                return RedirectToAction("Index");
            }

            // Convert base64 → byte[]
            var base64Data = anhKhuonMatBase64.Split(',')[1];
            var bytes = Convert.FromBase64String(base64Data);

            // Lưu ảnh
            string folderPath = Path.Combine(_env.WebRootPath, "uploads/chamcong");
            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            var fileName = $"{Guid.NewGuid()}.png";
            var savePath = Path.Combine(folderPath, fileName);
            System.IO.File.WriteAllBytes(savePath, bytes);

            // ✅ Tạo bản ghi chấm công (dùng MaNhanVien chuẩn)
            var chamCong = new ChamCong
            {
                MaNhanVien = nhanVien.MaNhanVien,
                NgayGioVaoCa = DateTime.Now,
                NgayGioKetCa = null,
                AnhNhanDienKhuonMat = "/uploads/chamcong/" + fileName,
                GhiChu = "Check-in bằng camera"
            };

            try
            {
                _context.ChamCong.Add(chamCong);
                _context.SaveChanges();
                TempData["Success"] = $"Bạn đã check-in lúc {chamCong.NgayGioVaoCa:HH:mm:ss dd/MM/yyyy}";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Lỗi khi lưu dữ liệu: " + ex.InnerException?.Message;
            }

            return RedirectToAction("LichSuNhanVien");
        }


        // Nhân viên Check-out
        [HttpPost]
        public IActionResult CheckOut()
        {
            var username = HttpContext.Session.GetString("TenDangNhap");
            if (string.IsNullOrEmpty(username))
                return RedirectToAction("DangNhap", "TaiKhoan");

            var nhanVien = _context.NhanVien.FirstOrDefault(nv => nv.TaiKhoanDangNhap == username);
            if (nhanVien == null) return RedirectToAction("Index");

            var chamCong = _context.ChamCong
                                   .Where(c => c.MaNhanVien == nhanVien.MaNhanVien && c.NgayGioKetCa == null)
                                   .OrderByDescending(c => c.NgayGioVaoCa)
                                   .FirstOrDefault();

            if (chamCong == null)
            {
                TempData["Error"] = "Bạn chưa check-in, không thể check-out!";
                return RedirectToAction("LichSuNhanVien");
            }

            chamCong.NgayGioKetCa = DateTime.Now; // ✅ luôn lấy giờ hiện tại
            chamCong.GhiChu = "Check-out tự động";

            _context.ChamCong.Update(chamCong);
            _context.SaveChanges();

            TempData["Success"] = $"Bạn đã check-out lúc {chamCong.NgayGioKetCa:HH:mm:ss dd/MM/yyyy}";
            return RedirectToAction("LichSuNhanVien");
        }

        // ✅ Lịch sử cho nhân viên (chỉ thấy của chính mình)
        public IActionResult LichSuNhanVien()
        {
            var username = HttpContext.Session.GetString("TenDangNhap");
            var role = HttpContext.Session.GetString("Role")?.Trim();

            if (string.IsNullOrEmpty(username))
            {
                TempData["Error"] = "Bạn chưa đăng nhập!";
                return RedirectToAction("DangNhap", "TaiKhoan");
            }

            // Nếu là Admin → chuyển sang Lịch sử admin
            if (string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase))
                return RedirectToAction("LichSuAdmin");

            var nhanVien = _context.NhanVien.FirstOrDefault(nv => nv.TaiKhoanDangNhap == username);
            if (nhanVien == null)
            {
                TempData["Error"] = "Không tìm thấy thông tin nhân viên!";
                return RedirectToAction("Index");
            }

            var lichSu = _context.ChamCong
                                 .Where(c => c.MaNhanVien == nhanVien.MaNhanVien)
                                 .OrderByDescending(c => c.NgayGioVaoCa)
                                 .ToList();

            return View(lichSu);
        }

        // Lịch sử admin

        [Authorize(Roles = "Admin")]
        public IActionResult LichSuAdmin(DateTime? tuNgay, DateTime? denNgay, int? maNhanVien)
        {
            // Lấy danh sách chấm công có include thông tin nhân viên
            var query = _context.ChamCong.Include(c => c.NhanVien).AsQueryable();

            if (tuNgay.HasValue)
                query = query.Where(c => c.NgayGioVaoCa.Date >= tuNgay.Value.Date);

            if (denNgay.HasValue)
                query = query.Where(c => c.NgayGioVaoCa.Date <= denNgay.Value.Date);

            if (maNhanVien.HasValue)
                query = query.Where(c => c.MaNhanVien == maNhanVien.Value);

            // Dùng ViewBag để gửi danh sách nhân viên cho filter
            ViewBag.NhanViens = _context.NhanVien.ToList();

            return View(query.OrderByDescending(c => c.NgayGioVaoCa).ToList());
        }
        // Đặt đoạn code này vào file ChamCongController.cs của bạn
        // =======================================
        // Xuất PDF (Đã cập nhật giao diện và font chữ đẹp hơn)
        // =======================================
        [Authorize(Roles = "Admin")]
        public IActionResult XuatPdfAdmin(DateTime? tuNgay, DateTime? denNgay, int? maNhanVien)
        {
            var query = _context.ChamCong.Include(c => c.NhanVien).AsQueryable();

            if (tuNgay.HasValue)
                query = query.Where(c => c.NgayGioVaoCa.Date >= tuNgay.Value.Date);

            if (denNgay.HasValue)
                query = query.Where(c => c.NgayGioVaoCa.Date <= denNgay.Value.Date);

            if (maNhanVien.HasValue)
                query = query.Where(c => c.MaNhanVien == maNhanVien.Value);

            var datas = query.OrderByDescending(c => c.NgayGioVaoCa).ToList();

            using (MemoryStream stream = new MemoryStream())
            {
                // Sử dụng A4 ngang (Rotate) để hiển thị bảng rộng hơn
                Document doc = new Document(PageSize.A4.Rotate(), 30f, 30f, 30f, 30f);
                PdfWriter.GetInstance(doc, stream);
                doc.Open();

                // --- Thiết lập Font cho tiếng Việt ---
                string fontPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/fonts/arial.ttf");

                // Xử lý trường hợp không tìm thấy font
                BaseFont bf;
                try
                {
                    bf = BaseFont.CreateFont(fontPath, BaseFont.IDENTITY_H, BaseFont.EMBEDDED);
                }
                catch
                {
                    // Fallback nếu không tìm thấy Arial
                    bf = BaseFont.CreateFont(BaseFont.HELVETICA, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
                }

                iTextSharp.text.Font fontTitle = new iTextSharp.text.Font(bf, 18, iTextSharp.text.Font.BOLD, new BaseColor(0, 123, 255)); // Xanh dương
                iTextSharp.text.Font fontHeader = new iTextSharp.text.Font(bf, 12, iTextSharp.text.Font.BOLD, BaseColor.WHITE);
                iTextSharp.text.Font fontNormal = new iTextSharp.text.Font(bf, 10, iTextSharp.text.Font.NORMAL);
                iTextSharp.text.Font fontSummary = new iTextSharp.text.Font(bf, 11, iTextSharp.text.Font.BOLD, BaseColor.RED);


                // --- 1. TIÊU ĐỀ ---
                Paragraph title = new Paragraph(new Chunk("BÁO CÁO LỊCH SỬ CHẤM CÔNG", fontTitle))
                {
                    Alignment = Element.ALIGN_CENTER,
                    SpacingAfter = 15f
                };
                doc.Add(title);

                // --- 2. THÔNG TIN LỌC ---
                string filterInfo = $"Phạm vi: {(tuNgay?.ToString("dd/MM/yyyy") ?? "Tất cả")} - {(denNgay?.ToString("dd/MM/yyyy") ?? "Tất cả")}";

                // Thêm tên nhân viên đã lọc (nếu có)
                if (maNhanVien.HasValue)
                {
                    var nv = _context.NhanVien.FirstOrDefault(x => x.MaNhanVien == maNhanVien.Value);
                    if (nv != null)
                        filterInfo += $" | Nhân viên: {nv.HoTen}";
                }

                Paragraph pFilter = new Paragraph(new Chunk(filterInfo, fontNormal))
                {
                    SpacingAfter = 10f,
                    Alignment = Element.ALIGN_LEFT
                };
                doc.Add(pFilter);


                // --- 3. BẢNG DỮ LIỆU ---
                PdfPTable table = new PdfPTable(4) { WidthPercentage = 100 };
                // Điều chỉnh độ rộng cột
                table.SetWidths(new float[] { 20f, 30f, 30f, 20f });

                string[] headers = { "Nhân viên", "Ngày giờ vào ca", "Ngày giờ kết ca", "Ghi chú" };

                // Headers - Màu xanh đậm
                foreach (var h in headers)
                {
                    PdfPCell cell = new PdfPCell(new Phrase(h, fontHeader))
                    {
                        BackgroundColor = new BaseColor(0, 123, 255), // Màu xanh dương (Primary)
                        HorizontalAlignment = Element.ALIGN_CENTER,
                        VerticalAlignment = Element.ALIGN_MIDDLE,
                        Padding = 8
                    };
                    table.AddCell(cell);
                }

                // Data rows
                foreach (var item in datas)
                {
                    // Tên nhân viên
                    table.AddCell(new PdfPCell(new Phrase(item.NhanVien?.HoTen ?? "-", fontNormal)) { Padding = 5, BorderColor = BaseColor.LIGHT_GRAY });
                    // Vào ca
                    table.AddCell(new PdfPCell(new Phrase(item.NgayGioVaoCa.ToString("dd/MM/yyyy HH:mm"), fontNormal)) { HorizontalAlignment = Element.ALIGN_CENTER, Padding = 5, BorderColor = BaseColor.LIGHT_GRAY });
                    // Kết ca (Dùng dấu gạch ngang nếu null)
                    table.AddCell(new PdfPCell(new Phrase(item.NgayGioKetCa?.ToString("dd/MM/yyyy HH:mm") ?? "-", fontNormal)) { HorizontalAlignment = Element.ALIGN_CENTER, Padding = 5, BorderColor = BaseColor.LIGHT_GRAY });
                    // Ghi chú
                    table.AddCell(new PdfPCell(new Phrase(item.GhiChu ?? "-", fontNormal)) { Padding = 5, BorderColor = BaseColor.LIGHT_GRAY });
                }

                doc.Add(table);

                // --- 4. TỔNG KẾT ---
                doc.Add(new Paragraph(" ", fontNormal) { SpacingAfter = 15f }); // Khoảng cách

                var totalSummary = new Paragraph($"Tổng số lượt chấm công được lọc: {datas.Count}", fontSummary)
                {
                    Alignment = Element.ALIGN_RIGHT
                };
                doc.Add(totalSummary);


                doc.Close();

                byte[] pdfBytes = stream.ToArray();
                return File(pdfBytes, "application/pdf", $"BaoCaoChamCong_{DateTime.Now.ToString("yyyyMMdd_HHmmss")}.pdf");
            }
        }



    }
}
