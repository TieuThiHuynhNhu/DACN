using DACN.Models;

namespace DACN.Repositories
{
    public interface IThanhToanRepository
    {
        Task<List<ThanhToan>> GetAllAsync();
        Task<ThanhToan> GetByIdAsync(int id);
        Task AddAsync(ThanhToan entity);
        Task UpdateAsync(ThanhToan entity);
        Task DeleteAsync(int id);
    }

}
