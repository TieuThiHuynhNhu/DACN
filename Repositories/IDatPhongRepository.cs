using DACN.Models;

namespace DACN.Repositories
{
    public interface IDatPhongRepository
    {
        Task<List<DatPhong>> GetAllAsync();
        Task<DatPhong> GetByIdAsync(int id);
        Task AddAsync(DatPhong entity);
        Task UpdateAsync(DatPhong entity);
        Task DeleteAsync(int id);
    }

}
