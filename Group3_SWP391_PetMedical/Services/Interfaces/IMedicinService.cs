using Group3_SWP391_PetMedical.Models;
using Group3_SWP391_PetMedical.Models.Common;

namespace Group3_SWP391_PetMedical.Services.Interfaces
{
    public interface IMedicinService
    {
        Task<PagedResult<Medication>> GetMedicinListAsync(PagingQuery query);
    }
}
