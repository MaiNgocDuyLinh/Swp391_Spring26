using Group3_SWP391_PetMedical.Models;
using Group3_SWP391_PetMedical.Models.Common;

namespace Group3_SWP391_PetMedical.Repository.Interfaces
{
    public interface IFeedbackRepository
    {
        Task<PagedResult<Feedback>> GetPagedAsync(string? search, int? starFilter, int page, int pageSize);
        Task<List<Feedback>> GetTopFeedbacksAsync(int count);
        Task<Feedback?> GetByIdAsync(int id);
    }
}
