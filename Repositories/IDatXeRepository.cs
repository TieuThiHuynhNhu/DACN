using DACN.Models;

namespace DACN.Repositories
{
    public interface IDatXeRepository
    {
        Task<List<DatXe>> GetAllAsync();
        Task<DatXe> GetByIdAsync(int id);
        Task AddAsync(DatXe datXe);
        Task UpdateAsync(DatXe datXe);
        Task DeleteAsync(int id);
    }

}
