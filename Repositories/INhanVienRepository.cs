using DACN.Models;

namespace DACN.Repositories
{
    public interface INhanVienRepository
    {
        Task<List<NhanVien>> GetAllAsync();
        Task<NhanVien> GetByIdAsync(int id);
        Task AddAsync(NhanVien entity);
        Task UpdateAsync(NhanVien entity);
        Task DeleteAsync(int id);
    }

}
