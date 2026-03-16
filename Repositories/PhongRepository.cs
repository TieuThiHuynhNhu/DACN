using DACN.Models;
using Microsoft.EntityFrameworkCore;

namespace DACN.Repositories
{
    // PhongRepository.cs
    public class PhongRepository : IPhongRepository
    {
        private readonly ApplicationDbContext _context;
        public PhongRepository(ApplicationDbContext context) => _context = context;

        public async Task<List<Phong>> GetAllAsync() => await _context.Phong.ToListAsync();
        public async Task<Phong> GetByIdAsync(int id) => await _context.Phong.FindAsync(id);
        public async Task AddAsync(Phong entity) { _context.Phong.Add(entity); await _context.SaveChangesAsync(); }
        public async Task UpdateAsync(Phong entity) { _context.Phong.Update(entity); await _context.SaveChangesAsync(); }
        public async Task DeleteAsync(int id) { var p = await GetByIdAsync(id); if (p != null) { _context.Phong.Remove(p); await _context.SaveChangesAsync(); } }
    }

}
