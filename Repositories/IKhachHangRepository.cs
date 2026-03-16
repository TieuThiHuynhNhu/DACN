using DACN.Models;

namespace DACN.Repositories
{
    public interface IKhachHangRepository
    {
        Task<List<KhachHang>> GetAllAsync();
        Task<KhachHang> GetByIdAsync(int id);
        Task AddAsync(KhachHang entity);
        Task UpdateAsync(KhachHang entity);
        Task DeleteAsync(int id);
    }

}
