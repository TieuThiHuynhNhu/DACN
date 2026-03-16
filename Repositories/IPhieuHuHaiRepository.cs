using DACN.Models;
namespace DACN.Repositories
{
    public interface IPhieuHuHaiRepository
    {
        Task<List<PhieuHuHai>> GetAllAsync();
        Task<PhieuHuHai> GetByIdAsync(int id);
        Task AddAsync(PhieuHuHai entity);
        Task UpdateAsync(PhieuHuHai entity);
        Task DeleteAsync(int id);
    }

}
