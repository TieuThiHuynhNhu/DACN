using DACN.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore; // Cần thiết để sử dụng .Include()
using System.Linq;

namespace DACN.Controllers
{
    public class AdminBookingController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminBookingController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 📋 Danh sách đặt phòng (có tìm kiếm & lọc)
        public IActionResult Index(string search, string paymentType, string customerName)
        {
            // Phải có .Include() để KhachHang được nạp và có thể truy cập HoTen
            var bookings = _context.DatPhong
                .Include(b => b.KhachHang)
                .Include(b => b.Phong)
                .AsQueryable();

            if (!string.IsNullOrEmpty(search))
                bookings = bookings.Where(b =>
                    b.MaDatPhong.ToString().Contains(search) ||
                    (b.GhiChu != null && b.GhiChu.Contains(search)));

            // Cập nhật: Xử lý tìm kiếm theo Tên Khách hàng
            if (!string.IsNullOrEmpty(customerName))
                bookings = bookings.Where(b =>
                    b.KhachHang != null && b.KhachHang.HoTen.Contains(customerName));

            if (!string.IsNullOrEmpty(paymentType))
                bookings = bookings.Where(b => b.HinhThucThanhToan == paymentType);

            var list = bookings
                .OrderByDescending(b => b.NgayDat)
                .ToList();

            ViewBag.Search = search;
            ViewBag.PaymentType = paymentType;
            // Cập nhật: Lưu CustomerName vào ViewBag để giữ giá trị trên form
            ViewBag.CustomerName = customerName;

            return View(list);
        }

        // 📝 Sửa đơn đặt phòng
        [HttpGet]
        public IActionResult Edit(int id)
        {
            // *** CẬP NHẬT: Sử dụng .Include() để nạp Khách hàng và Phòng ***
            var booking = _context.DatPhong
                .Include(b => b.KhachHang) // Nạp thông tin Khách hàng
                .Include(b => b.Phong)     // Nạp thông tin Phòng
                .FirstOrDefault(b => b.MaDatPhong == id);

            if (booking == null)
                return NotFound();

            return View(booking);
        }

        [HttpPost]
        public IActionResult Edit(DatPhong model)
        {
            var booking = _context.DatPhong.FirstOrDefault(b => b.MaDatPhong == model.MaDatPhong);
            if (booking == null)
                return NotFound();

            // Cập nhật các trường
            booking.GhiChu = model.GhiChu;
            booking.HinhThucThanhToan = model.HinhThucThanhToan;
            booking.NgayDat = model.NgayDat;
            booking.NgayNhan = model.NgayNhan;

            _context.SaveChanges();
            TempData["Message"] = "✔️ Cập nhật đơn đặt phòng thành công!";
            return RedirectToAction("Index");
        }

        // ❌ Xóa đơn đặt phòng
        [HttpGet]
        public IActionResult Delete(int id)
        {
            var booking = _context.DatPhong.FirstOrDefault(b => b.MaDatPhong == id);
            if (booking == null)
                return NotFound();

            _context.DatPhong.Remove(booking);
            _context.SaveChanges();
            TempData["Message"] = "🗑️ Xóa đơn đặt phòng thành công!";
            return RedirectToAction("Index");
        }
    }
}