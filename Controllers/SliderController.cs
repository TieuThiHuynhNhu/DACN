using DACN.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DACN.Controllers
{
    [Authorize(Roles = "Admin")]
    public class SliderController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;

        public SliderController(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _context.Sliders.OrderByDescending(s => s.Id).ToListAsync());
        }

        public IActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SliderModel model)
        {
            if (ModelState.IsValid)
            {
                if (model.HinhAnhFile != null)
                {
                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                    var ext = Path.GetExtension(model.HinhAnhFile.FileName).ToLowerInvariant();
                    if (!allowedExtensions.Contains(ext))
                    {
                        ModelState.AddModelError("HinhAnhFile", "Chỉ chấp nhận các định dạng .png, .jpg, .jpeg, .gif");
                        return View(model);
                    }

                    string uploadsFolder = Path.Combine(_env.WebRootPath, "hinh_ks");
                    if (!Directory.Exists(uploadsFolder))
                        Directory.CreateDirectory(uploadsFolder);

                    string uniqueFileName = Guid.NewGuid() + "_" + model.HinhAnhFile.FileName;
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await model.HinhAnhFile.CopyToAsync(stream);
                    }

                    model.HinhAnh = uniqueFileName;
                }

                _context.Sliders.Add(model);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(model);
        }

        public IActionResult Edit(int id)
        {
            var slider = _context.Sliders.Find(id);
            if (slider == null) return NotFound();
            return View(slider);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, SliderModel model)
        {
            if (id != model.Id) return NotFound();

            var existingSlider = _context.Sliders.Find(id);
            if (existingSlider == null) return NotFound();

            if (ModelState.IsValid)
            {
                existingSlider.Name = model.Name;
                existingSlider.MoTa = model.MoTa;
                existingSlider.HienThi = model.HienThi;

                if (model.HinhAnhFile != null)
                {
                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                    var ext = Path.GetExtension(model.HinhAnhFile.FileName).ToLowerInvariant();
                    if (!allowedExtensions.Contains(ext))
                    {
                        ModelState.AddModelError("HinhAnhFile", "Chỉ chấp nhận các định dạng .png, .jpg, .jpeg, .gif");
                        return View(model);
                    }

                    string uploadsFolder = Path.Combine(_env.WebRootPath, "hinh_ks");
                    string uniqueFileName = Guid.NewGuid() + "_" + model.HinhAnhFile.FileName;
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await model.HinhAnhFile.CopyToAsync(stream);
                    }

                    // Xóa ảnh cũ
                    if (!string.IsNullOrEmpty(existingSlider.HinhAnh))
                    {
                        var oldPath = Path.Combine(uploadsFolder, existingSlider.HinhAnh);
                        if (System.IO.File.Exists(oldPath))
                            System.IO.File.Delete(oldPath);
                    }

                    existingSlider.HinhAnh = uniqueFileName;
                }

                _context.Update(existingSlider);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(model);
        }

        public IActionResult Delete(int id)
        {
            var slider = _context.Sliders.Find(id);
            if (slider == null) return NotFound();

            // Xóa file ảnh
            if (!string.IsNullOrEmpty(slider.HinhAnh))
            {
                var filePath = Path.Combine(_env.WebRootPath, "hinh_ks", slider.HinhAnh);
                if (System.IO.File.Exists(filePath))
                    System.IO.File.Delete(filePath);
            }

            _context.Sliders.Remove(slider);
            _context.SaveChanges();
            return RedirectToAction(nameof(Index));
        }
    }
}
