using Group3_SWP391_PetMedical.Models;
using Group3_SWP391_PetMedical.Models.Common;

namespace Group3_SWP391_PetMedical.Repository.Interfaces
{
    public interface IMedicinRepository
    {
        Task<PagedResult<Medication>> GetPagedAsync(string? search, int page, int pageSize);
        Task<Medication?> GetByIdAsync(int id);
        Task<Medication> AddAsync(string name, decimal unitPrice, int stockQuantity, string? description);
        Task<bool> UpdateAsync(int id, string name, decimal unitPrice, int stockQuantity, string? description);
    }
}
