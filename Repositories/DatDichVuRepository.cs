using DACN.Models;
using Microsoft.EntityFrameworkCore;
namespace DACN.Repositories
{
    public class DatDichVuRepository : IDatDichVuRepository
    {
        private readonly ApplicationDbContext _context;
        public DatDichVuRepository(ApplicationDbContext context) => _context = context;

        public async Task<List<DatDichVu>> GetAllAsync() => await _context.DatDichVu.ToListAsync();
        public async Task<DatDichVu> GetByIdAsync(int id) => await _context.DatDichVu.FindAsync(id);
        public async Task AddAsync(DatDichVu entity) { _context.DatDichVu.Add(entity); await _context.SaveChangesAsync(); }
        public async Task UpdateAsync(DatDichVu entity) { _context.DatDichVu.Update(entity); await _context.SaveChangesAsync(); }
        public async Task DeleteAsync(int id) { var e = await GetByIdAsync(id); if (e != null) { _context.DatDichVu.Remove(e); await _context.SaveChangesAsync(); } }
    }

}
