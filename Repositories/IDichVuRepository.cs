using DACN.Models;

namespace DACN.Repositories
{
    public interface IDichVuRepository
    {
        Task<List<DichVu>> GetAllAsync();
        Task<DichVu> GetByIdAsync(int id);
        Task AddAsync(DichVu entity);
        Task UpdateAsync(DichVu entity);
        Task DeleteAsync(int id);
    }

}
