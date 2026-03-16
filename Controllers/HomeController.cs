using DACN.Models;
using DACN.Models.ViewModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;

namespace DACN.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            // 🖼️ Slider
            var sliders = _context.Sliders
                .Where(s => s.HienThi == 1)
                .OrderByDescending(s => s.Id)
                .ToList();

            // 🎉 Khuyến mãi
            var khuyenMai = _context.KhuyenMai
                .Include(k => k.Phong)
                .Where(k => k.NgayBatDau <= DateTime.Now && k.NgayKetThuc >= DateTime.Now)
                .OrderByDescending(k => k.PhanTramGiam)
                .Take(5)
                .ToList();

            // 🏨 Phòng trống + ưu tiên khuyến mãi
            var phongList = _context.Phong
                .Include(p => p.PhongKhuyenMais)
                    .ThenInclude(pk => pk.KhuyenMai)
                .Where(p => p.TrangThai == "ConTrong")
                .AsEnumerable() // xử lý trên client để tính KM
                .Select(p =>
                {
                    // Lấy khuyến mãi cao nhất đang áp dụng
                    var km = p.PhongKhuyenMais
                        .Where(pk => pk.KhuyenMai.NgayBatDau <= DateTime.Now
                                  && pk.KhuyenMai.NgayKetThuc >= DateTime.Now)
                        .OrderByDescending(pk => pk.KhuyenMai.PhanTramGiam)
                        .Select(pk => pk.KhuyenMai)
                        .FirstOrDefault();

                    if (km != null)
                    {
                        p.GiaGoc = p.GiaPhong;
                        p.GiaPhong = p.GiaPhong * (1 - (decimal)km.PhanTramGiam / 100);
                        p.KhuyenMaiHienTai = km; // dùng để hiển thị badge
                    }

                    return p;
                })
                // Ưu tiên phòng có KM
                .OrderByDescending(p => p.KhuyenMaiHienTai != null)
                .ThenBy(p => p.GiaPhong)
                .Take(6)
                .ToList();

            // 💆 Dịch vụ nổi bật
            var dichVuList = _context.DichVu
    .Where(d => d.TrangThai != null &&
                d.TrangThai.ToLower() == "conhoatdong".ToLower())
    .OrderByDescending(d => d.MaDichVu)
    .Take(6)
    .ToList();





            // ✅ Nếu null thì tạo list rỗng
            dichVuList ??= new List<DichVu>();

            // 🏷️ Danh sách loại phòng
            var loaiPhongs = _context.Phong
                .Select(p => p.LoaiPhong)
                .Distinct()
                .ToList();

            // ✅ Đóng gói vào ViewModel
            var viewModel = new HomeIndexViewModel
            {
                Sliders = sliders,
                KhuyenMais = khuyenMai,
                PhongList = phongList,
                DichVuList = dichVuList, // ✅ BẠN ĐÃ BỎ QUÊN DÒNG NÀY !!!
                LoaiPhongs = loaiPhongs
            };

            return View(viewModel);
        }

        [HttpPost]
        public IActionResult DatXe(DatXe model)
        {
            if (ModelState.IsValid)
            {
                _context.DatXe.Add(model);
                _context.SaveChanges();
                TempData["Success"] = "Đặt xe thành công!";
                return RedirectToAction("Index");
            }

            TempData["Error"] = "Có lỗi khi đặt xe.";
            return RedirectToAction("Index");
        }

    }
}
