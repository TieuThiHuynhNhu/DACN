using DACN.Models;
using Microsoft.EntityFrameworkCore;

namespace DACN.Repositories
{
    public class DatXeRepository : IDatXeRepository
    {
        private readonly ApplicationDbContext _context;

        public DatXeRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<DatXe>> GetAllAsync()
        {
            return await _context.DatXe.Include(d => d.KhachHang).ToListAsync();
        }

        public async Task<DatXe> GetByIdAsync(int id)
        {
            return await _context.DatXe.FindAsync(id);
        }

        public async Task AddAsync(DatXe datXe)
        {
            _context.DatXe.Add(datXe);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(DatXe datXe)
        {
            _context.DatXe.Update(datXe);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var entity = await _context.DatXe.FindAsync(id);
            if (entity != null)
            {
                _context.DatXe.Remove(entity);
                await _context.SaveChangesAsync();
            }
        }
    }

}
