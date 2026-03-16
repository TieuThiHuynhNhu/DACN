using DACN.Models;
using Microsoft.EntityFrameworkCore;
namespace DACN.Repositories
{
    public class ChamCongRepository : IChamCongRepository
    {
        private readonly ApplicationDbContext _context;
        public ChamCongRepository(ApplicationDbContext context) => _context = context;

        public async Task<List<ChamCong>> GetAllAsync() => await _context.ChamCong.ToListAsync();
        public async Task<ChamCong> GetByIdAsync(int id) => await _context.ChamCong.FindAsync(id);
        public async Task AddAsync(ChamCong entity) { _context.ChamCong.Add(entity); await _context.SaveChangesAsync(); }
        public async Task UpdateAsync(ChamCong entity) { _context.ChamCong.Update(entity); await _context.SaveChangesAsync(); }
        public async Task DeleteAsync(int id) { var e = await GetByIdAsync(id); if (e != null) { _context.ChamCong.Remove(e); await _context.SaveChangesAsync(); } }
    }

}
