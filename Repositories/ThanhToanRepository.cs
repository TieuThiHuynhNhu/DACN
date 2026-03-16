using DACN.Models;
using Microsoft.EntityFrameworkCore;
namespace DACN.Repositories
{
    public class ThanhToanRepository : IThanhToanRepository
    {
        private readonly ApplicationDbContext _context;
        public ThanhToanRepository(ApplicationDbContext context) => _context = context;

        public async Task<List<ThanhToan>> GetAllAsync() => await _context.ThanhToan.ToListAsync();
        public async Task<ThanhToan> GetByIdAsync(int id) => await _context.ThanhToan.FindAsync(id);
        public async Task AddAsync(ThanhToan entity) { _context.ThanhToan.Add(entity); await _context.SaveChangesAsync(); }
        public async Task UpdateAsync(ThanhToan entity) { _context.ThanhToan.Update(entity); await _context.SaveChangesAsync(); }
        public async Task DeleteAsync(int id) { var t = await GetByIdAsync(id); if (t != null) { _context.ThanhToan.Remove(t); await _context.SaveChangesAsync(); } }
    }

}
