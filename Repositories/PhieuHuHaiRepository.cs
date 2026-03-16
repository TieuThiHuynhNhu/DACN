using DACN.Models;
using Microsoft.EntityFrameworkCore;
namespace DACN.Repositories
{
    public class PhieuHuHaiRepository : IPhieuHuHaiRepository
    {
        private readonly ApplicationDbContext _context;
        public PhieuHuHaiRepository(ApplicationDbContext context) => _context = context;

        public async Task<List<PhieuHuHai>> GetAllAsync() => await _context.PhieuHuHai.ToListAsync();
        public async Task<PhieuHuHai> GetByIdAsync(int id) => await _context.PhieuHuHai.FindAsync(id);
        public async Task AddAsync(PhieuHuHai entity) { _context.PhieuHuHai.Add(entity); await _context.SaveChangesAsync(); }
        public async Task UpdateAsync(PhieuHuHai entity) { _context.PhieuHuHai.Update(entity); await _context.SaveChangesAsync(); }
        public async Task DeleteAsync(int id) { var e = await GetByIdAsync(id); if (e != null) { _context.PhieuHuHai.Remove(e); await _context.SaveChangesAsync(); } }
    }

}
