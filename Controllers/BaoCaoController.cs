using DACN.Models;
using DACN.Repositories;
using Microsoft.AspNetCore.Mvc;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Text;

namespace DACN.Controllers
{
    public class BaoCaoController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IBaoCaoRepository _baoCaoRepo;

        public BaoCaoController(ApplicationDbContext context, IBaoCaoRepository baoCaoRepo)
        {
            _context = context;
            _baoCaoRepo = baoCaoRepo;

            // Đăng ký code pages để QuestPDF hỗ trợ text
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }

        // =========================
        // ✅ Trang danh sách báo cáo
        // =========================
        public IActionResult Index(int? thang, int? nam)
        {
            var list = _context.BaoCao.ToList();
            ViewBag.SelectedMonth = thang;
            ViewBag.SelectedYear = nam;
            return View(list);
        }

        // =========================
        // ✅ Tạo báo cáo mới
        // =========================
        public IActionResult TaoBaoCao()
        {
            ViewBag.LoaiBaoCaoList = new List<string>
            {
                "Doanh thu",
                "Công suất phòng",
                "Dịch vụ",
                "Khách hàng",
                "Đặt phòng",
                "Nhân viên"
            };
            return View();
        }

        [HttpPost]
        public IActionResult TaoBaoCao(BaoCao model)
        {
            if (ModelState.IsValid)
            {
                model.NgayTao = DateTime.Now;
                model.TrangThai = "Đã xử lý";
                _context.BaoCao.Add(model);
                _context.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.LoaiBaoCaoList = new List<string>
            {
                "Doanh thu",
                "Công suất phòng",
                "Dịch vụ",
                "Khách hàng",
                "Đặt phòng",
                "Nhân viên"
            };
            return View(model);
        }

        // =========================
        // ✅ Xuất PDF báo cáo
        // =========================
        public IActionResult XuatPDF(int maBaoCao, int? thang, int? nam)
        {
            var baoCao = _context.BaoCao.FirstOrDefault(b => b.MaBaoCao == maBaoCao);
            if (baoCao == null)
                return NotFound("Báo cáo không tồn tại");

            string noiDung = string.IsNullOrWhiteSpace(baoCao.ChiTiet) ? "Không có dữ liệu" : baoCao.ChiTiet;

            try
            {
                var pdfBytes = Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.A4);
                        page.Margin(40);

                        page.Header().AlignCenter().Text($"BÁO CÁO {baoCao.LoaiBaoCao?.ToUpper() ?? ""}")
                            .FontFamily("Arial").FontSize(18).SemiBold();

                        page.Content().Column(col =>
                        {
                            col.Spacing(8);
                            col.Item().Text($"📄 Mã báo cáo: {baoCao.MaBaoCao}").FontFamily("Arial");
                            col.Item().Text($"📅 Ngày tạo: {baoCao.NgayTao:dd/MM/yyyy}").FontFamily("Arial");
                            col.Item().Text($"👤 Người tạo: {baoCao.NguoiTao}").FontFamily("Arial");
                            col.Item().Text($"⏳ Phạm vi: Tháng {(thang.HasValue ? thang.Value.ToString() : "Tất cả")}, Năm {(nam.HasValue ? nam.Value.ToString() : "Tất cả")}")
                                .FontFamily("Arial");
                            col.Item().Text("📝 Nội dung chi tiết:").SemiBold().FontFamily("Arial");
                            col.Item().Text(noiDung).FontFamily("Arial");
                        });

                        page.Footer().AlignRight().Text($"Ngày in: {DateTime.Now:dd/MM/yyyy HH:mm}")
                            .FontFamily("Arial").FontSize(10);
                    });
                }).GeneratePdf();

                string fileName = $"BaoCao_{baoCao.MaBaoCao}_{DateTime.Now:yyyyMMddHHmm}.pdf";
                return File(pdfBytes, "application/pdf", fileName);
            }
            catch (Exception ex)
            {
                return Content("Không thể tạo PDF: " + ex.Message);
            }
        }

        // =========================
        // Các phương thức phụ trợ
        // =========================
        private decimal TongDoanhThuDatPhong(int? thang, int? nam)
        {
            var query = _context.DatPhong.AsQueryable();
            if (thang.HasValue) query = query.Where(x => x.NgayDat.Month == thang.Value);
            if (nam.HasValue) query = query.Where(x => x.NgayDat.Year == nam.Value);
            return query.Sum(x => x.TongTien);
        }

        private decimal TongDoanhThuDichVu(int? thang, int? nam)
        {
            var query = _context.DonThanhToanDichVu.AsQueryable();
            if (thang.HasValue) query = query.Where(x => x.NgayTao.Month == thang.Value);
            if (nam.HasValue) query = query.Where(x => x.NgayTao.Year == nam.Value);
            return query.Sum(x => x.TongTien);
        }

        private double CongSuatPhong(int? thang, int? nam)
        {
            var phongCount = _context.Phong.Count();
            if (phongCount == 0) return 0;

            var datPhongQuery = _context.DatPhong.AsQueryable();
            if (thang.HasValue) datPhongQuery = datPhongQuery.Where(x => x.NgayDat.Month == thang.Value);
            if (nam.HasValue) datPhongQuery = datPhongQuery.Where(x => x.NgayDat.Year == nam.Value);

            return ((double)datPhongQuery.Count() / phongCount) * 100;
        }

        private string DichVuPhoBien(int? thang, int? nam)
        {
            var query = _context.ChiTietThanhToanDV.AsQueryable();
            if (thang.HasValue) query = query.Where(ct => ct.DonThanhToanDichVu.NgayTao.Month == thang.Value);
            if (nam.HasValue) query = query.Where(ct => ct.DonThanhToanDichVu.NgayTao.Year == nam.Value);

            var dvPhoBien = query.GroupBy(ct => ct.MaDichVu)
                                 .OrderByDescending(g => g.Count())
                                 .Select(g => (int?)g.Key)
                                 .FirstOrDefault();

            return dvPhoBien.HasValue ? dvPhoBien.Value.ToString() : "Không có dữ liệu";
        }
    }
}
