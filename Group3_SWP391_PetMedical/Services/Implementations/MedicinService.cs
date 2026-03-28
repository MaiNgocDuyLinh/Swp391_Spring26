using Group3_SWP391_PetMedical.Models;
using Group3_SWP391_PetMedical.Models.Common;
using Group3_SWP391_PetMedical.Repository.Interfaces;
using Group3_SWP391_PetMedical.Services.Interfaces;

namespace Group3_SWP391_PetMedical.Services.Implementations
{
    public class MedicinService : IMedicinService
    {
        private readonly IMedicinRepository _repo;

        public MedicinService(IMedicinRepository repo)
        {
            _repo = repo;
        }

        public Task<PagedResult<Medication>> GetMedicinListAsync(PagingQuery query)
            => _repo.GetPagedAsync(query.Q, query.Page, query.PageSize, query.Status);

        public Task<Medication?> GetByIdAsync(int id)
            => _repo.GetByIdAsync(id);

        public Task<Medication> AddAsync(string name, decimal unitPrice, int stockQuantity, string? description, string status)
            => _repo.AddAsync(name, unitPrice, stockQuantity, description, status);

        public Task<bool> UpdateAsync(int id, string name, decimal unitPrice, int stockQuantity, string? description, string status)
            => _repo.UpdateAsync(id, name, unitPrice, stockQuantity, description, status);

        public Task<Medication?> GetByNameAsync(string name)
            => _repo.GetByNameAsync(name);
    }
}
