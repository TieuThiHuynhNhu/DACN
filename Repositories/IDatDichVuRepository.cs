using DACN.Models;

namespace DACN.Repositories
{
    public interface IDatDichVuRepository
    {
        Task<List<DatDichVu>> GetAllAsync();
        Task<DatDichVu> GetByIdAsync(int id);
        Task AddAsync(DatDichVu entity);
        Task UpdateAsync(DatDichVu entity);
        Task DeleteAsync(int id);
    }

}
