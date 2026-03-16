using DACN.Models;
using Microsoft.EntityFrameworkCore;
namespace DACN.Repositories
{
    public class BaoCaoRepository : IBaoCaoRepository
    {
        private readonly ApplicationDbContext _context;
        public BaoCaoRepository(ApplicationDbContext context) => _context = context;

        public async Task<List<BaoCao>> GetAllAsync() => await _context.BaoCao.ToListAsync();
        public async Task<BaoCao> GetByIdAsync(int id) => await _context.BaoCao.FindAsync(id);
        public async Task AddAsync(BaoCao entity) { _context.BaoCao.Add(entity); await _context.SaveChangesAsync(); }
        public async Task UpdateAsync(BaoCao entity) { _context.BaoCao.Update(entity); await _context.SaveChangesAsync(); }
        public async Task DeleteAsync(int id) { var e = await GetByIdAsync(id); if (e != null) { _context.BaoCao.Remove(e); await _context.SaveChangesAsync(); } }
    }

}
