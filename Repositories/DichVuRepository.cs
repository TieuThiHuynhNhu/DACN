using DACN.Models;
using Microsoft.EntityFrameworkCore;
namespace DACN.Repositories
{
    public class DichVuRepository : IDichVuRepository
    {
        private readonly ApplicationDbContext _context;
        public DichVuRepository(ApplicationDbContext context) => _context = context;

        public async Task<List<DichVu>> GetAllAsync() => await _context.DichVu.ToListAsync();
        public async Task<DichVu> GetByIdAsync(int id) => await _context.DichVu.FindAsync(id);
        public async Task AddAsync(DichVu entity) { _context.DichVu.Add(entity); await _context.SaveChangesAsync(); }
        public async Task UpdateAsync(DichVu entity) { _context.DichVu.Update(entity); await _context.SaveChangesAsync(); }
        public async Task DeleteAsync(int id) { var d = await GetByIdAsync(id); if (d != null) { _context.DichVu.Remove(d); await _context.SaveChangesAsync(); } }
    }

}
