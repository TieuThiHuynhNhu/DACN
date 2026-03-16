using DACN.Models;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IO;
using System.Net.Http;
using System.Text.Json; // Cần cho JsonSerializer

public class XeController : Controller
{
    private readonly ApplicationDbContext _context;
    private const decimal DonGiaKm = 5000;

    public XeController(ApplicationDbContext context)
    {
        _context = context;
    }

    // =======================================
    // GET: Trang đặt xe
    // =======================================
    [HttpGet]
    public IActionResult DatXe()
    {
        return View();
    }

    // =======================================
    // POST: Đặt xe (Phương thức cũ, có thể không dùng nếu dùng DatXeGrab)
    // =======================================
    [HttpPost]
    public async Task<IActionResult> DatXe(
          string diaDiemDi,
          string diaDiemDen,
          string loaiXe,
          string ghiChu,
          DateTime? ngayDat)
    {
        // Lấy đúng session mà bạn đã lưu trong TaiKhoanController
        var maKh = HttpContext.Session.GetInt32("MaKh");

        if (maKh == null)
        {
            TempData["Error"] = "Bạn phải đăng nhập trước khi đặt xe!";
            return RedirectToAction("DangNhap", "TaiKhoan");
        }

        // Lấy thông tin khách hàng
        var khach = await _context.KhachHang
       .FirstOrDefaultAsync(k => k.MaKhachHang == maKh);

        if (khach == null)
        {
            Console.WriteLine("❌ KHÔNG TÌM THẤY KHÁCH CÓ MaKh = " + maKh);
            TempData["Error"] = "Không tìm thấy thông tin khách hàng!";
            return RedirectToAction("DangNhap", "TaiKhoan");
        }

        Console.WriteLine("SESSION_MAKH = " + maKh);

        // Tính khoảng cách
        double km = await TinhKhoangCach(diaDiemDi, diaDiemDen);
        decimal giaTien = (decimal)km * 40000;

        // Ghi đơn đặt xe
        var datXe = new DatXe
        {
            MaKhachHang = maKh,
            TenKhachHang = khach.HoTen,
            DiaDiemDi = diaDiemDi,
            DiaDiemDen = diaDiemDen,
            KhoangCach = (decimal)km,
            LoaiXe = loaiXe,
            GhiChu = ghiChu,
            NgayDat = ngayDat ?? DateTime.Now,
            GiaTien = giaTien
        };

        _context.DatXe.Add(datXe);
        await _context.SaveChangesAsync();

        TempData["Success"] = $"Đặt xe thành công! Tổng tiền: {giaTien:N0} VNĐ";
        return RedirectToAction("Index", "Home");
    }


    // ============================
    // Chuyển địa chỉ → Tọa độ
    // ============================
    public async Task<(double lat, double lon)> GetToaDo(string address)
    {
        string url = $"https://nominatim.openstreetmap.org/search?q={Uri.EscapeDataString(address)}&format=json&limit=1";

        using var http = new HttpClient();
        http.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0");

        var response = await http.GetStringAsync(url);
        var json = JsonDocument.Parse(response);

        if (json.RootElement.GetArrayLength() == 0)
            return (0, 0);

        var item = json.RootElement[0];
        return (
            double.Parse(item.GetProperty("lat").GetString()),
            double.Parse(item.GetProperty("lon").GetString())
        );
    }

    // ============================
    // API tính km
    // ============================
    public async Task<double> TinhKhoangCach(string di, string den)
    {
        var from = await GetToaDo(di);
        var to = await GetToaDo(den);

        if (from.lat == 0 || to.lat == 0)
            return 0;

        string url = $"https://router.project-osrm.org/route/v1/driving/{from.lon},{from.lat};{to.lon},{to.lat}?overview=false";

        using var http = new HttpClient();
        var response = await http.GetStringAsync(url);
        var json = JsonDocument.Parse(response);

        if (!json.RootElement.TryGetProperty("routes", out var routes))
            return 0;

        if (routes.GetArrayLength() == 0)
            return 0;

        return routes[0].GetProperty("distance").GetDouble() / 1000;
    }

    // =======================================
    // ⭐ POST: Đặt xe Grab (Dùng trong DatXe.cshtml) ⭐
    // =======================================
    [HttpPost]
    public async Task<IActionResult> DatXeGrab([FromBody] DatXe model)
    {
        var maKh = HttpContext.Session.GetInt32("MaKh");

        if (maKh == null)
            // Trả về JSON với URL chuyển hướng đến trang đăng nhập nếu chưa đăng nhập
            return Json(new { success = false, message = "Bạn cần đăng nhập!", redirectUrl = Url.Action("DangNhap", "TaiKhoan") });

        var kh = await _context.KhachHang.FirstOrDefaultAsync(x => x.MaKhachHang == maKh);

        if (kh == null)
            return Json(new { success = false, message = "Không tìm thấy thông tin khách!" });

        if (string.IsNullOrEmpty(model.LoaiXe))
            return Json(new { success = false, message = "Thiếu loại xe!" });

        var dat = new DatXe
        {
            MaKhachHang = maKh,
            TenKhachHang = kh.HoTen,
            DiaDiemDi = model.DiaDiemDi,
            DiaDiemDen = model.DiaDiemDen,
            KhoangCach = model.KhoangCach,
            GiaTien = model.GiaTien,
            LoaiXe = model.LoaiXe,
            NgayDat = DateTime.Now,
            GhiChu = model.GhiChu
        };

        _context.DatXe.Add(dat);
        await _context.SaveChangesAsync();

        // 💡 Lưu thông tin chuyến đi vào TempData để hiển thị ở trang khác
        TempData["DatXeSuccess"] = true;
        TempData["DatXeInfo"] = JsonSerializer.Serialize(new
        {
            LoaiXe = model.LoaiXe,
            GiaTien = model.GiaTien.ToString("N0"),
            Di = model.DiaDiemDi,
            Den = model.DiaDiemDen
        });

        // 💡 Trả về JSON thành công kèm URL chuyển hướng đến trang xác nhận mới
        return Json(new { success = true, message = "Đặt xe thành công!", redirectUrl = Url.Action("DatXeThanhCong", "Xe") });
    }

    // =======================================
    // ⭐ GET: Trang xác nhận đặt xe (Trang chuyển đến) ⭐
    // =======================================
    public IActionResult DatXeThanhCong()
    {
        // Kiểm tra và lấy thông tin từ TempData
        if (TempData["DatXeSuccess"] == null)
        {
            // Nếu không có dữ liệu, chuyển hướng về trang đặt xe
            TempData["Error"] = "Không tìm thấy thông tin chuyến đi.";
            return RedirectToAction("DatXe", "Xe");
        }

        // Đọc thông tin chuyến đi đã lưu từ TempData
        var infoJson = TempData["DatXeInfo"] as string;
        // Sử dụng JsonElement để đọc các trường dữ liệu
        var info = JsonSerializer.Deserialize<JsonElement>(infoJson);

        // Truyền thông tin này sang View để hiển thị chi tiết cho khách hàng
        ViewBag.LoaiXe = info.GetProperty("LoaiXe").GetString();
        ViewBag.GiaTien = info.GetProperty("GiaTien").GetString();
        ViewBag.Di = info.GetProperty("Di").GetString();
        ViewBag.Den = info.GetProperty("Den").GetString();

        return View(); // View này phải được tạo: Views/Xe/DatXeThanhCong.cshtml
    }


    // ============================
    // Lịch sử KH
    // ============================
    public IActionResult LichSuKhach()
    {
        var maKh = HttpContext.Session.GetInt32("MaKh");
        if (maKh == null)
            return RedirectToAction("DangNhap", "TaiKhoan");

        var data = _context.DatXe
            .Where(d => d.MaKhachHang == maKh)
            .OrderByDescending(d => d.NgayDat)
            .ToList();

        return View(data);
    }


    // ============================
    // Admin xem lịch sử
    // ============================
    [Authorize(Roles = "Admin,NhanVien")]
    public IActionResult LichSuAdmin(DateTime? tuNgay, DateTime? denNgay, string tenKhachHang)
    {
        var query = _context.DatXe.AsQueryable();

        if (tuNgay != null)
            query = query.Where(d => d.NgayDat.Date >= tuNgay.Value.Date);

        if (denNgay != null)
            query = query.Where(d => d.NgayDat.Date <= denNgay.Value.Date);

        if (!string.IsNullOrEmpty(tenKhachHang))
            query = query.Where(d => d.TenKhachHang.Contains(tenKhachHang));

        return View(query.OrderByDescending(d => d.NgayDat).ToList());
    }
    // =======================================
    // Xuất PDF
    // =======================================
    // Đặt đoạn code này vào file XeController.cs của bạn
    // =======================================
    // Xuất PDF (Đã cập nhật giao diện và tổng tiền)
    // =======================================
    [Authorize(Roles = "Admin,NhanVien")]
    public IActionResult XuatPdf(DateTime? tuNgay, DateTime? denNgay, string tenKhachHang)
    {
        var query = _context.DatXe.AsQueryable();

        if (tuNgay.HasValue)
            query = query.Where(d => d.NgayDat.Date >= tuNgay.Value.Date);

        if (denNgay.HasValue)
            query = query.Where(d => d.NgayDat.Date <= denNgay.Value.Date);

        if (!string.IsNullOrEmpty(tenKhachHang))
            query = query.Where(d => d.TenKhachHang.Contains(tenKhachHang));

        var datas = query.OrderByDescending(d => d.NgayDat).ToList();

        // Tính tổng kết
        var totalCount = datas.Count;
        var tongTien = datas.Sum(d => d.GiaTien);

        using var stream = new MemoryStream();
        // Đổi PageSize.A4 thành PageSize.A4.Rotate() để bảng rộng hơn
        var doc = new Document(PageSize.A4.Rotate(), 20f, 20f, 20f, 20f);
        PdfWriter.GetInstance(doc, stream);
        doc.Open();

        // --- Thiết lập Font cho tiếng Việt ---
        var fontPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/fonts/arial.ttf");
        if (!System.IO.File.Exists(fontPath))
        {
            // Fallback font nếu không tìm thấy Arial
            var fallbackFont = FontFactory.GetFont(FontFactory.HELVETICA, 10, BaseColor.BLACK);
            doc.Add(new Paragraph("Lịch sử đặt xe - Lỗi font chữ", new iTextSharp.text.Font(fallbackFont.BaseFont, 16, iTextSharp.text.Font.BOLD))
            {
                Alignment = Element.ALIGN_CENTER,
                SpacingAfter = 15f
            });
            doc.Close();
            return File(stream.ToArray(), "application/pdf", "LichSuDatXe_LoiFont.pdf");
        }

        var bf = BaseFont.CreateFont(fontPath, BaseFont.IDENTITY_H, BaseFont.EMBEDDED);
        var fontHeader = new iTextSharp.text.Font(bf, 14, iTextSharp.text.Font.BOLD, BaseColor.BLUE);
        var fontTitle = new iTextSharp.text.Font(bf, 16, iTextSharp.text.Font.BOLD);
        var fontNormal = new iTextSharp.text.Font(bf, 10);
        var fontTotal = new iTextSharp.text.Font(bf, 11, iTextSharp.text.Font.BOLD, BaseColor.RED);
        var fontTableHeader = new iTextSharp.text.Font(bf, 11, iTextSharp.text.Font.BOLD, BaseColor.WHITE);

        // --- 1. TIÊU ĐỀ BÁO CÁO ---
        doc.Add(new Paragraph("BÁO CÁO LỊCH SỬ ĐẶT XE", fontTitle)
        {
            Alignment = Element.ALIGN_CENTER,
            SpacingAfter = 15f
        });

        // Thêm khoảng thời gian lọc (nếu có)
        string filterRange = "Tất cả";
        if (tuNgay.HasValue && denNgay.HasValue)
        {
            filterRange = $"Từ ngày {tuNgay.Value.ToString("dd/MM/yyyy")} đến ngày {denNgay.Value.ToString("dd/MM/yyyy")}";
        }
        else if (tuNgay.HasValue)
        {
            filterRange = $"Từ ngày {tuNgay.Value.ToString("dd/MM/yyyy")}";
        }
        else if (denNgay.HasValue)
        {
            filterRange = $"Đến ngày {denNgay.Value.ToString("dd/MM/yyyy")}";
        }

        doc.Add(new Paragraph($"Phạm vi: {filterRange}", fontNormal)
        {
            Alignment = Element.ALIGN_CENTER,
            SpacingAfter = 10f
        });


        // --- 2. BẢNG DỮ LIỆU ---
        // Sử dụng 8 cột (Thêm cột Ngày đặt)
        PdfPTable table = new PdfPTable(8) { WidthPercentage = 100 };
        // Điều chỉnh độ rộng cột để phù hợp với định dạng ngang (Rotate)
        table.SetWidths(new float[] { 5, 12, 20, 20, 8, 10, 15, 10 });

        string[] headers = { "STT", "Khách hàng", "Điểm đi", "Điểm đến", "Km", "Loại xe", "Giá (VNĐ)", "Ngày đặt" };

        foreach (var h in headers)
        {
            var cell = new PdfPCell(new Phrase(h, fontTableHeader))
            {
                BackgroundColor = new BaseColor(13, 110, 253), // Màu xanh Primary
                HorizontalAlignment = Element.ALIGN_CENTER,
                VerticalAlignment = Element.ALIGN_MIDDLE,
                Padding = 6
            };
            table.AddCell(cell);
        }

        int stt = 1;
        foreach (var d in datas)
        {
            // Tách chuỗi địa điểm tương tự trong View (đảm bảo tính nhất quán)
            string diaDiemDiHienThi = d.DiaDiemDi;
            string diaDiemDenHienThi = d.DiaDiemDen;

            if (string.IsNullOrWhiteSpace(d.DiaDiemDen) || d.DiaDiemDen.Length < 5 || d.DiaDiemDi.Equals(d.DiaDiemDen, StringComparison.OrdinalIgnoreCase))
            {
                int splitIndex = -1;
                int indexTruongDaiHoc = d.DiaDiemDi.IndexOf("Trường Đại học", StringComparison.OrdinalIgnoreCase);
                if (indexTruongDaiHoc > 0)
                {
                    splitIndex = indexTruongDaiHoc;
                }
                else
                {
                    int indexDaiHoc = d.DiaDiemDi.IndexOf("Đại học", StringComparison.OrdinalIgnoreCase);
                    if (indexDaiHoc > 0)
                    {
                        splitIndex = indexDaiHoc;
                    }
                }

                if (splitIndex > 0)
                {
                    diaDiemDiHienThi = d.DiaDiemDi.Substring(0, splitIndex).Trim();
                    diaDiemDenHienThi = d.DiaDiemDi.Substring(splitIndex).Trim();
                }
            }

            // Dữ liệu các dòng
            table.AddCell(new PdfPCell(new Phrase(stt.ToString(), fontNormal)) { HorizontalAlignment = Element.ALIGN_CENTER });
            table.AddCell(new PdfPCell(new Phrase(d.TenKhachHang, fontNormal)));
            table.AddCell(new PdfPCell(new Phrase(diaDiemDiHienThi, fontNormal)));
            table.AddCell(new PdfPCell(new Phrase(diaDiemDenHienThi, fontNormal)));
            table.AddCell(new PdfPCell(new Phrase(d.KhoangCach.ToString("0.##"), fontNormal)) { HorizontalAlignment = Element.ALIGN_RIGHT });
            table.AddCell(new PdfPCell(new Phrase(d.LoaiXe ?? "-", fontNormal)) { HorizontalAlignment = Element.ALIGN_CENTER });
            table.AddCell(new PdfPCell(new Phrase(d.GiaTien.ToString("N0"), fontNormal)) { HorizontalAlignment = Element.ALIGN_RIGHT });
            table.AddCell(new PdfPCell(new Phrase(d.NgayDat.ToString("dd/MM/yyyy"), fontNormal)) { HorizontalAlignment = Element.ALIGN_CENTER });
            stt++;
        }

        doc.Add(table);

        // --- 3. TỔNG KẾT DOANH THU ---
        doc.Add(new Paragraph(" ", fontNormal) { SpacingAfter = 15f }); // Khoảng cách

        // Tổng số chuyến
        var totalTrips = new Paragraph($"Tổng số chuyến: {totalCount}", fontTotal)
        {
            Alignment = Element.ALIGN_RIGHT
        };
        doc.Add(totalTrips);

        // Tổng doanh thu
        var totalRevenue = new Paragraph($"TỔNG DOANH THU: {tongTien.ToString("N0")} VNĐ", fontTotal)
        {
            Alignment = Element.ALIGN_RIGHT
        };
        doc.Add(totalRevenue);

        doc.Close();

        return File(stream.ToArray(), "application/pdf", $"BaoCaoLichSuDatXe_{DateTime.Now.ToString("yyyyMMdd_HHmmss")}.pdf");
    }
}