
using DACN.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;
using System.Globalization;
using DACN.Models.ViewModel;
namespace DACN.Controllers
{
    public class ThongKeController : Controller
    {
        private readonly ApplicationDbContext _context;
        public ThongKeController(ApplicationDbContext context)
        {
            _context = context;
        }


        // 🔹 TRANG TỔNG HỢP
        // GET: /ThongKe/ThongKeTongHop
        public async Task<IActionResult> Index(int? thang, int? nam)
        {
            // Giữ lại logic lọc cơ bản hoặc chỉ hiển thị tổng quan/biểu đồ ở đây nếu có
            // ...

            // Chỉ cần truyền tháng/năm đang chọn
            var model = new ThongKeTongHopViewModel
            {
                SelectedMonth = thang,
                SelectedYear = nam
            };

            // Quay lại View(model) cũ hoặc tạo một View Dashboard mới
            return View(model);
        }

        // 🔸 1. Báo cáo Doanh thu (Đặt phòng và Dịch vụ)
        [HttpGet]
        public async Task<IActionResult> BaoCaoDoanhThu(int? thang, int? nam)
        {
            // 1. Lọc đơn đặt phòng: 
            // - Loại bỏ các đơn "DaHuy" và đơn đang chờ thanh toán
            var queryPhong = _context.DatPhong
                .Include(x => x.KhachHang)
                .Include(x => x.Phong)
                .Where(p => p.TrangThai != "DaHuy" && p.TrangThai != "ChoThanhToan")
                .AsQueryable();

            // 2. Lọc đơn dịch vụ từ bảng DatDichVu
            // ⭐ CẬP NHẬT: Chỉ tính doanh thu cho các đơn đã thanh toán hoặc đã hoàn tất phục vụ
            var queryDV = _context.DatDichVu
                .Include(x => x.DichVu)
                .Include(x => x.KhachHang)
                .Where(d => d.TrangThai == "DatThanhCong" || d.TrangThai == "HoanTat")
                .AsQueryable();

            // 3. Lọc theo thời gian
            if (thang.HasValue)
            {
                queryPhong = queryPhong.Where(p => p.NgayDat.Month == thang.Value);
                queryDV = queryDV.Where(p => p.NgaySuDung.HasValue && p.NgaySuDung.Value.Month == thang.Value);
            }
            if (nam.HasValue)
            {
                queryPhong = queryPhong.Where(p => p.NgayDat.Year == nam.Value);
                queryDV = queryDV.Where(p => p.NgaySuDung.HasValue && p.NgaySuDung.Value.Year == nam.Value);
            }

            // 4. Đóng gói dữ liệu vào ViewModel
            var model = new ThongKeTongHopViewModel
            {
                DatPhongs = await queryPhong.ToListAsync(),
                LichSuDichVu = await queryDV.ToListAsync(),
                SelectedMonth = thang,
                SelectedYear = nam
            };

            // 5. Tính toán tổng tiền thực tế
            ViewBag.TongDoanhThuPhong = model.DatPhongs.Sum(x => x.TongTien);
            ViewBag.TongDoanhThuDichVu = model.LichSuDichVu.Sum(x => x.ThanhTien);
            ViewBag.TongCong = (decimal)ViewBag.TongDoanhThuPhong + (decimal)ViewBag.TongDoanhThuDichVu;

            return View(model);
        }

        // 🔸 2. Báo cáo Phiếu Hư Hại
        public async Task<IActionResult> BaoCaoHuHai(int? thang, int? nam)
        {
            var queryHuHai = _context.PhieuHuHai.AsQueryable();

            if (thang.HasValue)
                queryHuHai = queryHuHai.Where(p => p.ThoiGianGhiNhan.Month == thang.Value);
            if (nam.HasValue)
                queryHuHai = queryHuHai.Where(p => p.ThoiGianGhiNhan.Year == nam.Value);

            var model = new ThongKeTongHopViewModel
            {
                PhieuHuHais = await queryHuHai.ToListAsync(),
                SelectedMonth = thang,
                SelectedYear = nam
            };

            return View(model); // Tạo View BaoCaoHuHai.cshtml
        }

        // 🔸 3. Báo cáo Chấm Công
        public async Task<IActionResult> BaoCaoChamCong(int? thang, int? nam)
        {
            var queryChamCong = _context.ChamCong.Include(c => c.NhanVien).AsQueryable();

            if (thang.HasValue)
                queryChamCong = queryChamCong.Where(c => c.NgayGioVaoCa.Month == thang.Value);
            if (nam.HasValue)
                queryChamCong = queryChamCong.Where(c => c.NgayGioVaoCa.Year == nam.Value);

            var model = new ThongKeTongHopViewModel
            {
                ChamCongs = await queryChamCong.ToListAsync(),
                SelectedMonth = thang,
                SelectedYear = nam
            };

            return View(model); // Tạo View BaoCaoChamCong.cshtml
        }

        // =================== XUẤT PDF ===================

        // 🔸 Doanh thu đặt phòng
        public IActionResult XuatPdfPhong(int? thang, int? nam)
        {
            var query = _context.DatPhong.Include(p => p.KhachHang).Include(p => p.Phong).AsQueryable();

            if (thang.HasValue) query = query.Where(x => x.NgayDat.Month == thang);
            if (nam.HasValue) query = query.Where(x => x.NgayDat.Year == nam);

            var list = query.ToList();

            var rows = list.Select(x => new string[]
            {
        x.Phong?.LoaiPhong ?? "",
        x.KhachHang?.HoTen ?? "",
        x.NgayDat.ToString("dd/MM/yyyy"),
        x.TongTien.ToString("N0")
            }).ToList();

            decimal total = list.Sum(x => x.TongTien);


            return TaoPdfTable(
                "BÁO CÁO ĐẶT PHÒNG - SEAHOTEL",
                rows,
                new[] { "Phòng", "Khách hàng", "Ngày đặt", "Tổng tiền" },
                total
            );
        }


        // 🔸 Doanh thu dịch vụ
        // 🔸 PDF Doanh thu dịch vụ (Đã sửa để khớp với bảng DatDichVu và logic trong Canvas)
        public IActionResult XuatPdfDichVu(int? thang, int? nam)
        {
            // 1. Truy vấn từ bảng DatDichVu thay vì DonThanhToanDichVu
            var query = _context.DatDichVu
                .Include(x => x.DichVu)
                .Include(x => x.KhachHang)
                // 2. Đồng bộ logic: Chỉ tính các đơn đã xác nhận (DatThanhCong) hoặc hoàn tất (HoanTat)
                .Where(d => d.TrangThai == "DatThanhCong" || d.TrangThai == "HoanTat")
                .AsQueryable();

            // 3. Lọc theo ngày sử dụng thực tế (giống như trên giao diện báo cáo)
            if (thang.HasValue)
                query = query.Where(x => x.NgaySuDung.HasValue && x.NgaySuDung.Value.Month == thang);
            if (nam.HasValue)
                query = query.Where(x => x.NgaySuDung.HasValue && x.NgaySuDung.Value.Year == nam);

            var list = query.ToList();

            // 4. Mapping dữ liệu vào mảng string để đưa vào bảng PDF
            var rows = list.Select(x => new string[]
            {
        x.MaDatDichVu.ToString(),                  // Mã lịch đặt
        x.KhachHang?.HoTen ?? "Khách lẻ",           // Tên khách hàng
        x.DichVu?.TenDichVu ?? "Dịch vụ",           // Tên dịch vụ
        x.NgaySuDung?.ToString("dd/MM/yyyy") ?? "", // Ngày dùng
        x.ThanhTien.ToString("N0")                  // Thành tiền (Doanh thu)
            }).ToList();

            // 5. Tính tổng tiền thực tế
            decimal total = list.Sum(x => x.ThanhTien);

            // 6. Trả về file PDF với tiêu đề và các cột đã được cập nhật
            return TaoPdfTable(
                "BÁO CÁO DOANH THU DỊCH VỤ - SEAHOTEL",
                rows,
                new[] { "Mã Lịch", "Khách hàng", "Dịch vụ", "Ngày dùng", "Thành tiền" },
                total
            );
        }
        // 🔸 Phiếu hư hại
        // 🔸 Phiếu hư hại
        public IActionResult XuatPdfHuHai(int? thang, int? nam)
        {
            var query = _context.PhieuHuHai.AsQueryable();

            if (thang.HasValue) query = query.Where(x => x.ThoiGianGhiNhan.Month == thang);
            if (nam.HasValue) query = query.Where(x => x.ThoiGianGhiNhan.Year == nam);

            var list = query.ToList();

            var rows = list.Select(x => new string[]
            {
        x.MaPhieu.ToString(),
        x.MaPhong.ToString(),
        // >>> ĐÃ THÊM CỘT THỜI GIAN GHI NHẬN <<<
        x.ThoiGianGhiNhan.ToString("dd/MM/yyyy HH:mm"), // Format ngày giờ
        // >>> HẾT THÊM CỘT <<<
        x.TenThietBi ?? "",
        x.MucDoHuHai ?? "",
        x.ChiPhiBoiThuong.ToString("N0"),
        x.TrangThai ?? ""
            }).ToList();

            decimal total = list.Sum(x => x.ChiPhiBoiThuong);


            return TaoPdfTable(
                "BÁO CÁO PHIẾU HƯ HẠI - SEAHOTEL",
                rows,
                // >>> CẬP NHẬT HEADER (7 cột) <<<
                new[] { "Mã", "Phòng", "Thời gian", "Thiết bị", "Mức độ", "Chi phí", "Trạng thái" },
                // >>> HẾT CẬP NHẬT HEADER <<<
                total
            );
        }


        // 🔸 Chấm công nhân viên
        public IActionResult XuatPdfChamCong(int? thang, int? nam)
        {
            var query = _context.ChamCong.Include(c => c.NhanVien).AsQueryable();
            if (thang.HasValue) query = query.Where(x => x.NgayGioVaoCa.Month == thang);
            if (nam.HasValue) query = query.Where(x => x.NgayGioVaoCa.Year == nam);

            var list = query.ToList();

            var rows = list.Select(x => new string[]
            {
        x.NhanVien?.HoTen ?? "-",
        x.NgayGioVaoCa.ToString("dd/MM/yyyy HH:mm"),
        x.NgayGioKetCa?.ToString("HH:mm") ?? "Chưa có",
        x.GhiChu ?? "-"
            }).ToList();

            return TaoPdfTable(
                "BÁO CÁO CHẤM CÔNG - SEAHOTEL",
                rows,
                new[] { "Nhân viên", "Vào ca", "Kết ca", "Ghi chú" },
                0  // Không có tổng tiền
            );
        }


        // 🔹 HÀM TẠO FILE PDF CHUNG
        // 🔹 HÀM TẠO FILE PDF CHUNG (Đã chỉnh sửa)
        // ... (Phần đầu hàm TaoPdfTable giữ nguyên)

        // 🔹 HÀM TẠO FILE PDF CHUNG (Đã chỉnh sửa để phân chia cột)
        private FileContentResult TaoPdfTable(
            string title,
            List<string[]> rows,
            string[] header,
            decimal totalMoney = 0
        )
        {
            using var stream = new MemoryStream();
            var pdf = new PdfDocument();
            var page = pdf.AddPage();

            // Khai báo gfx lần đầu (BẮT BUỘC)
            var gfx = XGraphics.FromPdfPage(page);

            // Đăng ký font (Bạn cần đảm bảo thư viện hỗ trợ Font Unicode)
            // Nếu bạn gặp lỗi font tiếng Việt, hãy nghiên cứu cách đăng ký FontResolver của PDFsharp
            var fontTitle = new XFont("Arial", 20, XFontStyle.Bold);
            var fontSubtitle = new XFont("Arial", 10, XFontStyle.Regular);
            var fontHeader = new XFont("Arial", 12, XFontStyle.Bold);
            var font = new XFont("Arial", 11, XFontStyle.Regular);
            var fontTotal = new XFont("Arial", 14, XFontStyle.Bold);

            // Khai báo Màu sắc
            var primaryColor = XBrushes.DarkBlue;
            var headerBgColor = XColors.LightBlue;
            var totalColor = XBrushes.Red;

            int y = 40;
            int rowHeight = 25;
            int startX = 40;
            int pageMargin = 40;
            int tableWidth = (int)(page.Width - 2 * pageMargin);

            // --- LOGIC PHÂN CHIA ĐỘ RỘNG CỘT MỚI ---

            // ... bên trong hàm TaoPdfTable

            // --- LOGIC PHÂN CHIA ĐỘ RỘNG CỘT MỚI ---

            double[] columnRatios;

            // Logic xác định tỷ lệ dựa trên số lượng cột (độ dài của header)
            if (header.Length == 4) // Báo cáo Đặt phòng
            {
                // Phân bổ độ rộng: 40% | 25% | 20% | 15%
                columnRatios = new double[] { 0.40, 0.25, 0.20, 0.15 };
            }
            else if (header.Length == 5) // Báo cáo Dịch vụ/Chấm công
            {
                // Phân bổ độ rộng: 10% | 25% | 25% | 20% | 20%
                columnRatios = new double[] { 0.10, 0.25, 0.25, 0.20, 0.20 };
            }
            else if (header.Length == 6) // Giữ logic này nếu có báo cáo 6 cột khác
            {
                // Phân bổ độ rộng: 10% | 25% | 15% | 10% | 20% | 20%
                columnRatios = new double[] { 0.10, 0.25, 0.15, 0.10, 0.20, 0.20 };
            }
            else if (header.Length == 7) // >>> LOGIC MỚI CHO BÁO CÁO HƯ HẠI (7 cột) <<<
            {
                // Phân bổ độ rộng: Mã | Phòng | Thời gian | Thiết bị | Mức độ | Chi phí | Trạng thái
                // Tỷ lệ: 8% | 15% | 20% | 15% | 10% | 15% | 17%
                columnRatios = new double[] { 0.08, 0.15, 0.20, 0.15, 0.10, 0.15, 0.17 };
            }
            else // Trường hợp mặc định (chia đều)
            {
                double equalRatio = 1.0 / header.Length;
                columnRatios = Enumerable.Repeat(equalRatio, header.Length).ToArray();
            }

            // ... (Phần còn lại của hàm TaoPdfTable giữ nguyên)

            // Tính toán độ rộng tuyệt đối của từng cột
            int[] columnWidths = columnRatios.Select(r => (int)(r * tableWidth)).ToArray();

            // --- 1. TIÊU ĐỀ VÀ THÔNG TIN CHUNG ---

            gfx.DrawString(title, fontTitle, primaryColor,
                new XRect(0, y, page.Width, 30), XStringFormats.TopCenter);
            y += 30;

            gfx.DrawString($"Thời gian báo cáo: {DateTime.Now:dd/MM/yyyy HH:mm}", fontSubtitle, XBrushes.Gray,
                new XRect(0, y, page.Width, 15), XStringFormats.TopCenter);
            y += 40;

            // --- 2. HEADER CỦA BẢNG ---

            int currentX = startX;

            // Vẽ nền cho Header
            gfx.DrawRectangle(new XSolidBrush(headerBgColor), startX, y, tableWidth, rowHeight);

            for (int i = 0; i < header.Length; i++)
            {
                int cw = columnWidths[i];

                // Vẽ viền
                gfx.DrawRectangle(XPens.DarkBlue, currentX, y, cw, rowHeight);

                // Vẽ chữ Header
                gfx.DrawString(header[i], fontHeader, primaryColor,
                    new XRect(currentX, y + 5, cw, rowHeight),
                    XStringFormats.TopCenter);

                currentX += cw;
            }

            y += rowHeight;

            // --- 3. DỮ LIỆU CỦA BẢNG ---

            foreach (var r in rows)
            {
                // Kiểm tra tràn trang
                if (y + rowHeight > page.Height - 60)
                {
                    page = pdf.AddPage();
                    // KHẮC PHỤC LỖI "gfx does not exist": GÁN LẠI ĐỐI TƯỢNG XGraphics
                    gfx = XGraphics.FromPdfPage(page);
                    y = 40;

                    // Vẽ lại Header trên trang mới
                    currentX = startX;
                    gfx.DrawRectangle(new XSolidBrush(headerBgColor), startX, y, tableWidth, rowHeight);
                    for (int i = 0; i < header.Length; i++)
                    {
                        int cw = columnWidths[i];
                        gfx.DrawRectangle(XPens.DarkBlue, currentX, y, cw, rowHeight);
                        gfx.DrawString(header[i], fontHeader, primaryColor,
                            new XRect(currentX, y + 5, cw, rowHeight),
                            XStringFormats.TopCenter);
                        currentX += cw;
                    }
                    y += rowHeight;
                }

                // Vẽ nội dung hàng
                currentX = startX;
                for (int i = 0; i < r.Length; i++)
                {
                    int cw = columnWidths[i];

                    // Vẽ viền ngoài cho ô
                    gfx.DrawRectangle(XPens.LightGray, currentX, y, cw, rowHeight);

                    // Căn chỉnh nội dung
                    XStringFormat format;
                    if (i == header.Length - 1 && totalMoney > 0)
                    {
                        format = XStringFormats.TopRight; // Cột cuối (Tiền) căn phải
                    }
                    else
                    {
                        format = XStringFormats.TopCenter; // Các cột khác căn giữa
                    }

                    // Vẽ chữ
                    gfx.DrawString(r[i], font, XBrushes.Black,
                        new XRect(currentX + 5, y + 5, cw - 10, rowHeight), // -10 để có padding 5px
                        format);

                    currentX += cw;
                }

                y += rowHeight;
            }

            // --- 4. TỔNG CỘNG (Nếu có) ---
            if (totalMoney > 0)
            {
                y += 10;

                // Tạo đường phân cách
                gfx.DrawLine(XPens.Black, startX, y, startX + tableWidth, y);
                y += 10;

                // Vẽ hàng tổng cộng (căn phải)
                string totalString = $"TỔNG CỘNG: {totalMoney:N0} VNĐ";

                gfx.DrawString(totalString, fontTotal, totalColor,
                    new XRect(startX, y, tableWidth, rowHeight),
                    XStringFormats.TopRight);
            }

            pdf.Save(stream, false);
            return File(stream.ToArray(), "application/pdf",
                $"{title.Replace(" ", "_")}_{DateTime.Now:yyyyMMddHHmm}.pdf");
        }

    }
}
