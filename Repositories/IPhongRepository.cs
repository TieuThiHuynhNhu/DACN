using DACN.Models;

namespace DACN.Repositories
{
    // IPhongRepository.cs
    public interface IPhongRepository
    {
        Task<List<Phong>> GetAllAsync();
        Task<Phong> GetByIdAsync(int id);
        Task AddAsync(Phong entity);
        Task UpdateAsync(Phong entity);
        Task DeleteAsync(int id);
    }

}
