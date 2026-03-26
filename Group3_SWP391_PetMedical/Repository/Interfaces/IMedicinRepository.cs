using Group3_SWP391_PetMedical.Models;
using Group3_SWP391_PetMedical.Models.Common;

namespace Group3_SWP391_PetMedical.Repository.Interfaces
{
    public interface IMedicinRepository
    {
        Task<PagedResult<Medication>> GetPagedAsync(string? search, int page, int pageSize, string? status = null);
        Task<Medication?> GetByIdAsync(int id);
        Task<Medication> AddAsync(string name, decimal unitPrice, int stockQuantity, string? description, string status);
        Task<bool> UpdateAsync(int id, string name, decimal unitPrice, int stockQuantity, string? description, string status);
        Task<Medication?> GetByNameAsync(string name);
    }
}
