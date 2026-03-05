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
            => _repo.GetPagedAsync(query.Q, query.Page, query.PageSize);
    }
}
