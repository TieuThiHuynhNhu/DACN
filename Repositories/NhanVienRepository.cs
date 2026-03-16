using DACN.Models;
using Microsoft.EntityFrameworkCore;
namespace DACN.Repositories
{
    public class NhanVienRepository : INhanVienRepository
    {
        private readonly ApplicationDbContext _context;
        public NhanVienRepository(ApplicationDbContext context) => _context = context;

        public async Task<List<NhanVien>> GetAllAsync() => await _context.NhanVien.ToListAsync();
        public async Task<NhanVien> GetByIdAsync(int id) => await _context.NhanVien.FindAsync(id);
        public async Task AddAsync(NhanVien entity) { _context.NhanVien.Add(entity); await _context.SaveChangesAsync(); }
        public async Task UpdateAsync(NhanVien entity) { _context.NhanVien.Update(entity); await _context.SaveChangesAsync(); }
        public async Task DeleteAsync(int id) { var n = await GetByIdAsync(id); if (n != null) { _context.NhanVien.Remove(n); await _context.SaveChangesAsync(); } }
    }

}
