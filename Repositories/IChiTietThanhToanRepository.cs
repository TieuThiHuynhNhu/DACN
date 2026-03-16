using DACN.Models;

namespace DACN.Repositories
{
    public interface IChiTietThanhToanRepository
    {
        Task<List<ChiTietThanhToan>> GetAllAsync();
        Task<ChiTietThanhToan> GetByIdAsync(int id);
        Task AddAsync(ChiTietThanhToan entity);
        Task DeleteAsync(int id);
    }

}
