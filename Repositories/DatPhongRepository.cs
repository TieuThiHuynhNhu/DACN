using DACN.Models;
using Microsoft.EntityFrameworkCore;

namespace DACN.Repositories
{
    public class DatPhongRepository : IDatPhongRepository
    {
        private readonly ApplicationDbContext _context;
        public DatPhongRepository(ApplicationDbContext context) => _context = context;

        public async Task<List<DatPhong>> GetAllAsync() => await _context.DatPhong.ToListAsync();
        public async Task<DatPhong> GetByIdAsync(int id) => await _context.DatPhong.FindAsync(id);
        public async Task AddAsync(DatPhong entity) { _context.DatPhong.Add(entity); await _context.SaveChangesAsync(); }
        public async Task UpdateAsync(DatPhong entity) { _context.DatPhong.Update(entity); await _context.SaveChangesAsync(); }
        public async Task DeleteAsync(int id) { var d = await GetByIdAsync(id); if (d != null) { _context.DatPhong.Remove(d); await _context.SaveChangesAsync(); } }
    }

}
