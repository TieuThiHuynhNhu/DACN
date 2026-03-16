
using DACN.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DACN.Repositories
{
    public class KhuyenMaiRepository : IKhuyenMaiRepository
    {
        private readonly ApplicationDbContext _context;

        public KhuyenMaiRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<KhuyenMai>> GetAllAsync()
        {
            return await _context.KhuyenMai
                .Include(k => k.Phong)
                .ToListAsync();
        }

        public async Task<KhuyenMai> GetByIdAsync(int id)
        {
            return await _context.KhuyenMai
                .Include(k => k.Phong)
                .FirstOrDefaultAsync(k => k.MaKhuyenMai == id);
        }

        public async Task AddAsync(KhuyenMai khuyenMai)
        {
            _context.KhuyenMai.Add(khuyenMai);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(KhuyenMai khuyenMai)
        {
            _context.KhuyenMai.Update(khuyenMai);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var km = await _context.KhuyenMai.FindAsync(id);
            if (km != null)
            {
                _context.KhuyenMai.Remove(km);
                await _context.SaveChangesAsync();
            }
        }
    }
}
