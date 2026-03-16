using DACN.Models;
using Microsoft.EntityFrameworkCore;
namespace DACN.Repositories
{
    public class KhachHangRepository : IKhachHangRepository
    {
        private readonly ApplicationDbContext _context;
        public KhachHangRepository(ApplicationDbContext context) => _context = context;

        public async Task<List<KhachHang>> GetAllAsync() => await _context.KhachHang.ToListAsync();
        public async Task<KhachHang> GetByIdAsync(int id) => await _context.KhachHang.FindAsync(id);
        public async Task AddAsync(KhachHang entity) { _context.KhachHang.Add(entity); await _context.SaveChangesAsync(); }
        public async Task UpdateAsync(KhachHang entity) { _context.KhachHang.Update(entity); await _context.SaveChangesAsync(); }
        public async Task DeleteAsync(int id) { var e = await GetByIdAsync(id); if (e != null) { _context.KhachHang.Remove(e); await _context.SaveChangesAsync(); } }
    }

}
