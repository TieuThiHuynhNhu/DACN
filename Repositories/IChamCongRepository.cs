using DACN.Models;

namespace DACN.Repositories
{
    public interface IChamCongRepository
    {
        Task<List<ChamCong>> GetAllAsync();
        Task<ChamCong> GetByIdAsync(int id);
        Task AddAsync(ChamCong entity);
        Task UpdateAsync(ChamCong entity);
        Task DeleteAsync(int id);
    }

}
