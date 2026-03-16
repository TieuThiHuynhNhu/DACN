using DACN.Models;
using Microsoft.EntityFrameworkCore;
namespace DACN.Repositories
{
    public class ChiTietThanhToanRepository : IChiTietThanhToanRepository
    {
        private readonly ApplicationDbContext _context;
        public ChiTietThanhToanRepository(ApplicationDbContext context) => _context = context;

        public async Task<List<ChiTietThanhToan>> GetAllAsync() => await _context.ChiTietThanhToan.ToListAsync();
        public async Task<ChiTietThanhToan> GetByIdAsync(int id) => await _context.ChiTietThanhToan.FindAsync(id);
        public async Task AddAsync(ChiTietThanhToan entity) { _context.ChiTietThanhToan.Add(entity); await _context.SaveChangesAsync(); }
        public async Task DeleteAsync(int id) { var e = await GetByIdAsync(id); if (e != null) { _context.ChiTietThanhToan.Remove(e); await _context.SaveChangesAsync(); } }
    }

}
