using DACN.Models;

namespace DACN.Repositories
{
    public interface IBaoCaoRepository
    {
        Task<List<BaoCao>> GetAllAsync();
        Task<BaoCao?> GetByIdAsync(int id);
        Task AddAsync(BaoCao entity);
        Task UpdateAsync(BaoCao entity);
        Task DeleteAsync(int id);
    }
}
