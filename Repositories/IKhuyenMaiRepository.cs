using DACN.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DACN.Repositories
{
    public interface IKhuyenMaiRepository
    {
        Task<IEnumerable<KhuyenMai>> GetAllAsync();
        Task<KhuyenMai> GetByIdAsync(int id);
        Task AddAsync(KhuyenMai khuyenMai);
        Task UpdateAsync(KhuyenMai khuyenMai);
        Task DeleteAsync(int id);
    }
}
