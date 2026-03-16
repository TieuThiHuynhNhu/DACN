using DACN.Models;
using DACN.Models.ViewModel;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IO;
using System.Linq;
using System;
using System.Globalization; // Cần thêm thư viện này
using System.Collections.Generic; // Cần thêm thư viện này

namespace DACN.Controllers
{
    public class LichSuDatController : Controller
    {
        private readonly ApplicationDbContext _context;

        public LichSuDatController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Trang hiển thị lịch sử đặt phòng (ĐÃ CẬP NHẬT)
        public IActionResult Index(string tenKhachHang, string thangNam)
        {
            string tenDangNhap = HttpContext.Session.GetString("TenDangNhap");
            if (string.IsNullOrEmpty(tenDangNhap))
                return RedirectToAction("DangNhap", "TaiKhoan");

            var taiKhoan = _context.TaiKhoan.FirstOrDefault(t => t.TenDangNhap == tenDangNhap);
            if (taiKhoan == null)
                return Unauthorized();

            var datPhongQuery = _context.DatPhong
                .Include(dp => dp.Phong)
                .Include(dp => dp.KhachHang)
                .AsQueryable();

            // Lọc theo Khách hàng nếu không phải Admin/NV
            if (taiKhoan.QuyenTruyCap != "Admin" && taiKhoan.QuyenTruyCap != "NhanVien")
            {
                var khachHang = _context.KhachHang.FirstOrDefault(kh => kh.TenDangNhap == tenDangNhap);
                if (khachHang == null)
                    return Unauthorized();

                datPhongQuery = datPhongQuery.Where(dp => dp.MaKhachHang == khachHang.MaKhachHang);
            }

            // ⭐ LOGIC LỌC MỚI CHO ADMIN/NHANVIEN ⭐

            // 1. Lọc theo Tên Khách hàng
            if (!string.IsNullOrEmpty(tenKhachHang))
            {
                // Kiểm tra xem KhachHang có tồn tại không trước khi gọi Contains
                datPhongQuery = datPhongQuery.Where(dp => dp.KhachHang != null && dp.KhachHang.HoTen.Contains(tenKhachHang));
            }

            // 2. Lọc theo Tháng/Năm (Dùng định dạng mm/yy hoặc mm/yyyy từ Datepicker)
            DateTime parsedDate;
            if (!string.IsNullOrEmpty(thangNam) &&
                (DateTime.TryParseExact(thangNam, "MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedDate) ||
                 DateTime.TryParseExact(thangNam, "MM/yy", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedDate)))
            {
                datPhongQuery = datPhongQuery.Where(dp =>
                    dp.NgayNhan.HasValue &&
                    dp.NgayNhan.Value.Month == parsedDate.Month &&
                    dp.NgayNhan.Value.Year == parsedDate.Year
                );
            }
            // Loại bỏ logic lọc theo 'ngay' cũ (vì đã chuyển sang thangNam)

            var datPhongList = datPhongQuery.ToList();

            // Lấy danh sách đánh giá hiện tại của khách (nếu có)
            var danhGiaList = new List<DanhGiaPhong>();
            if (taiKhoan.QuyenTruyCap != "Admin" && taiKhoan.QuyenTruyCap != "NhanVien")
            {
                var khachHangHienTai = _context.KhachHang.FirstOrDefault(kh => kh.TenDangNhap == tenDangNhap);
                if (khachHangHienTai != null)
                {
                    danhGiaList = _context.DanhGiaPhongs
                        .Where(dg => dg.MaKhachHang == khachHangHienTai.MaKhachHang)
                        .ToList();
                }
            }


            var viewModel = new LichSuDatViewModel
            {
                LichSuDatPhong = datPhongList,
                // Không cần NgayTimKiem nữa, nhưng giữ lại nếu View cần
                DanhGiaList = danhGiaList
            };

            // Truyền giá trị lọc hiện tại sang View để giữ trạng thái trên form
            ViewData["TenKhachHang"] = tenKhachHang;
            ViewData["ThangNam"] = thangNam;

            return View(viewModel);
        }

        // ... (Các Action ChiTiet và DanhGia giữ nguyên) ...

        public IActionResult ChiTiet(int id)
        {
            string tenDangNhap = HttpContext.Session.GetString("TenDangNhap");
            if (string.IsNullOrEmpty(tenDangNhap))
                return RedirectToAction("DangNhap", "TaiKhoan");

            var taiKhoan = _context.TaiKhoan.FirstOrDefault(t => t.TenDangNhap == tenDangNhap);
            if (taiKhoan == null)
                return Unauthorized();

            var datPhong = _context.DatPhong
                .Include(dp => dp.Phong)
                .Include(dp => dp.KhachHang)
                .FirstOrDefault(dp => dp.MaDatPhong == id);

            if (datPhong == null)
                return NotFound();

            // Kiểm tra quyền truy cập (Chỉ Admin/NV hoặc Khách hàng đặt mới được xem)
            if (taiKhoan.QuyenTruyCap != "Admin" && taiKhoan.QuyenTruyCap != "NhanVien")
            {
                var khachHang = _context.KhachHang.FirstOrDefault(kh => kh.TenDangNhap == tenDangNhap);
                if (khachHang == null || datPhong.MaKhachHang != khachHang.MaKhachHang)
                {
                    return Forbid(); // Cấm truy cập nếu không phải chủ đơn hàng
                }
            }
            return View(datPhong);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DanhGia(int maPhong, int soSao, string noiDung)
        {
            string tenDangNhap = HttpContext.Session.GetString("TenDangNhap");
            if (string.IsNullOrEmpty(tenDangNhap))
                return RedirectToAction("DangNhap", "TaiKhoan");

            var khachHang = _context.KhachHang.FirstOrDefault(kh => kh.TenDangNhap == tenDangNhap);
            if (khachHang == null)
                return Unauthorized();

            // Kiểm tra khách đã đặt phòng chưa
            var datPhong = _context.DatPhong.FirstOrDefault(dp => dp.MaPhong == maPhong && dp.MaKhachHang == khachHang.MaKhachHang);
            if (datPhong == null)
                return BadRequest("Bạn chưa đặt phòng này.");

            // Kiểm tra khách đã đánh giá chưa
            var danhGiaCu = _context.DanhGiaPhongs.FirstOrDefault(dg => dg.MaPhong == maPhong && dg.MaKhachHang == khachHang.MaKhachHang);
            if (danhGiaCu != null)
            {
                // Cập nhật đánh giá
                danhGiaCu.SoSao = soSao;
                danhGiaCu.NoiDung = noiDung;
                danhGiaCu.NgayDanhGia = DateTime.Now;
                _context.Update(danhGiaCu);
            }
            else
            {
                var danhGiaMoi = new DanhGiaPhong
                {
                    MaPhong = maPhong,
                    MaKhachHang = khachHang.MaKhachHang,
                    SoSao = soSao,
                    NoiDung = noiDung,
                    NgayDanhGia = DateTime.Now
                };
                _context.Add(danhGiaMoi);
            }

            _context.SaveChanges();
            TempData["Success"] = "Cảm ơn bạn đã đánh giá phòng!";
            return RedirectToAction("Index");
        }

        public IActionResult InHoaDonDatPhong(int id)
        {
            string tenDangNhap = HttpContext.Session.GetString("TenDangNhap");
            if (string.IsNullOrEmpty(tenDangNhap))
                return RedirectToAction("DangNhap", "TaiKhoan");

            var taiKhoan = _context.TaiKhoan.FirstOrDefault(t => t.TenDangNhap == tenDangNhap);
            if (taiKhoan == null)
                return Unauthorized();

            var datPhong = _context.DatPhong
                .Include(dp => dp.Phong)
                .Include(dp => dp.KhachHang)
                .FirstOrDefault(dp => dp.MaDatPhong == id);

            if (datPhong == null)
                return NotFound();

            // Kiểm tra quyền truy cập (giữ nguyên)
            if (taiKhoan.QuyenTruyCap != "Admin" && taiKhoan.QuyenTruyCap != "NhanVien")
            {
                var khachHang = _context.KhachHang.FirstOrDefault(kh => kh.TenDangNhap == tenDangNhap);
                if (khachHang == null || datPhong.MaKhachHang != khachHang.MaKhachHang)
                {
                    return Forbid();
                }
            }

            // --- LOGIC TÍNH TOÁN TIỀN ---
            decimal tongTien = datPhong.TongTien;
            int soDem = 0;

            if (datPhong.NgayNhan.HasValue && datPhong.NgayTra.HasValue)
            {
                TimeSpan duration = datPhong.NgayTra.Value.Date - datPhong.NgayNhan.Value.Date;
                soDem = duration.Days;
                if (soDem < 1) soDem = 1;

                // Nếu TongTien từ DB bị 0, tính toán lại
                if (tongTien <= 0)
                {
                    decimal giaPhongMoiDem = datPhong.Phong?.GiaPhong ?? 0;
                    tongTien = giaPhongMoiDem * soDem;
                }
            }
            else
            {
                soDem = 1;
            }

            ViewData["TongTien"] = tongTien;
            ViewData["SoDem"] = soDem;

            return View(datPhong);
        }

        // Xuất PDF (ĐÃ CẬP NHẬT)
        public IActionResult XuatPdf(string tenKhachHang, string thangNam)
        {
            var datasQuery = _context.DatPhong
                .Include(dp => dp.Phong)
                .Include(dp => dp.KhachHang)
                .AsQueryable();

            // ⭐ LOGIC LỌC MỚI CHO PDF ⭐

            // 1. Lọc theo Tên Khách hàng
            if (!string.IsNullOrEmpty(tenKhachHang))
            {
                datasQuery = datasQuery.Where(dp => dp.KhachHang != null && dp.KhachHang.HoTen.Contains(tenKhachHang));
            }

            // 2. Lọc theo Tháng/Năm
            DateTime parsedDate;
            if (!string.IsNullOrEmpty(thangNam) &&
                (DateTime.TryParseExact(thangNam, "MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedDate) ||
                 DateTime.TryParseExact(thangNam, "MM/yy", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedDate)))
            {
                datasQuery = datasQuery.Where(dp =>
                    dp.NgayNhan.HasValue &&
                    dp.NgayNhan.Value.Month == parsedDate.Month &&
                    dp.NgayNhan.Value.Year == parsedDate.Year
                );
            }

            var datas = datasQuery.ToList();

            using (MemoryStream stream = new MemoryStream())
            {
                Document doc = new Document(PageSize.A4, 20f, 20f, 20f, 20f);
                PdfWriter.GetInstance(doc, stream);
                doc.Open();

                // Font Unicode
                string fontPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/fonts/arial.ttf");
                // Cần đảm bảo file font arial.ttf tồn tại trong thư mục wwwroot/fonts
                BaseFont bf = BaseFont.CreateFont(fontPath, BaseFont.IDENTITY_H, BaseFont.EMBEDDED);
                iTextSharp.text.Font fontHeader = new iTextSharp.text.Font(bf, 16, iTextSharp.text.Font.BOLD);
                iTextSharp.text.Font font = new iTextSharp.text.Font(bf, 12, iTextSharp.text.Font.NORMAL);
                iTextSharp.text.Font fontBold = new iTextSharp.text.Font(bf, 12, iTextSharp.text.Font.BOLD);


                // Tiêu đề
                Paragraph title = new Paragraph(new Chunk("BÁO CÁO LỊCH SỬ ĐẶT PHÒNG - SEAHOTEL", fontHeader));
                title.Alignment = Element.ALIGN_CENTER;
                title.SpacingAfter = 15f;
                doc.Add(title);

                // Thông tin lọc
                string thongTinLoc = "";
                if (!string.IsNullOrEmpty(tenKhachHang)) thongTinLoc += $"Khách hàng: {tenKhachHang} | ";
                if (!string.IsNullOrEmpty(thangNam)) thongTinLoc += $"Tháng/Năm: {thangNam}";
                if (string.IsNullOrEmpty(thongTinLoc)) thongTinLoc = "Lọc: Tất cả";

                Paragraph pLoc = new Paragraph(new Chunk(thongTinLoc, font));
                doc.Add(pLoc);

                Paragraph pSoPhong = new Paragraph(new Chunk($"Tổng số đơn đặt phòng: {datas.Count}", font));
                pSoPhong.SpacingAfter = 10f;
                doc.Add(pSoPhong);

                // Bảng dữ liệu
                PdfPTable table = new PdfPTable(7) { WidthPercentage = 100 };
                table.SetWidths(new float[] { 10f, 20f, 20f, 15f, 15f, 15f, 10f });

                string[] headers = { "Mã DP", "Loại phòng", "Khách hàng", "Ngày nhận", "Ngày trả", "Tổng tiền", "Trạng thái" };
                foreach (var h in headers)
                {
                    PdfPCell cell = new PdfPCell(new Phrase(h, fontBold))
                    {
                        BackgroundColor = new BaseColor(135, 206, 250), // Sky Blue
                        HorizontalAlignment = Element.ALIGN_CENTER,
                        Padding = 5
                    };
                    table.AddCell(cell);
                }

                decimal tongDoanhThu = 0;
                foreach (var dp in datas)
                {
                    // Tính tổng tiền cho báo cáo (nếu TongTien trong DB bằng 0)
                    decimal tongTienDon = dp.TongTien;
                    if (tongTienDon <= 0 && dp.NgayNhan.HasValue && dp.NgayTra.HasValue)
                    {
                        TimeSpan duration = dp.NgayTra.Value.Date - dp.NgayNhan.Value.Date;
                        int soDem = duration.Days < 1 ? 1 : duration.Days;
                        tongTienDon = (dp.Phong?.GiaPhong ?? 0) * soDem;
                    }
                    tongDoanhThu += tongTienDon;

                    table.AddCell(new PdfPCell(new Phrase(dp.MaDatPhong.ToString(), font)));
                    table.AddCell(new PdfPCell(new Phrase(dp.Phong?.LoaiPhong ?? "-", font)));
                    table.AddCell(new PdfPCell(new Phrase(dp.KhachHang?.HoTen ?? "-", font)));
                    table.AddCell(new PdfPCell(new Phrase(dp.NgayNhan?.ToString("dd/MM/yyyy") ?? "-", font)));
                    table.AddCell(new PdfPCell(new Phrase(dp.NgayTra?.ToString("dd/MM/yyyy") ?? "-", font)));
                    table.AddCell(new PdfPCell(new Phrase(string.Format("{0:N0} VNĐ", tongTienDon), font)) { HorizontalAlignment = Element.ALIGN_RIGHT });


                    string trangThai = dp.TrangThai switch
                    {
                        "DaNhan" => "Đã nhận",
                        "DangCho" => "Đang chờ",
                        "DaHuy" => "Đã hủy",
                        _ => dp.TrangThai
                    };
                    table.AddCell(new PdfPCell(new Phrase(trangThai, font)));
                }

                doc.Add(table);

                // Tổng doanh thu (Chỉ tính những đơn không bị hủy)
                Paragraph pTongDoanhThu = new Paragraph(new Chunk($"TỔNG TIỀN ĐÃ LỌC: {string.Format("{0:N0} VNĐ", tongDoanhThu)}", fontBold));
                pTongDoanhThu.Alignment = Element.ALIGN_RIGHT;
                pTongDoanhThu.SpacingBefore = 10f;
                doc.Add(pTongDoanhThu);

                doc.Close();

                byte[] pdfBytes = stream.ToArray();
                return File(pdfBytes, "application/pdf", $"BaoCaoDatPhong_{DateTime.Now:yyyyMMdd_HHmmss}.pdf");
            }
        }
    }
}