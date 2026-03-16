using DACN.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DACN.Controllers
{
    public class ThongBaoController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ThongBaoController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var list = await _context.ThongBao
                .OrderByDescending(t => t.ThoiGian)
                .ToListAsync();
            return View(list);
        }

        [HttpPost]
        public async Task<IActionResult> DanhDauDaDoc(int id)
        {
            var tb = await _context.ThongBao.FindAsync(id);
            if (tb != null)
            {
                tb.DaDoc = true;
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Details(int id)
        {
            var tb = await _context.ThongBao.FindAsync(id);
            if (tb == null)
                return NotFound();

            // Khi mở chi tiết thì đánh dấu là "Đã đọc"
            if (!tb.DaDoc)
            {
                tb.DaDoc = true;
                await _context.SaveChangesAsync();
            }

            return View(tb);
        }
    }
}
