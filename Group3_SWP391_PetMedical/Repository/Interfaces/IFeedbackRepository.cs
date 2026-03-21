using Group3_SWP391_PetMedical.Models.Common;
using Group3_SWP391_PetMedical.ViewModels.Feedback;
using Group3_SWP391_PetMedical.Models;


namespace Group3_SWP391_PetMedical.Repository.Interfaces
{
    public interface IFeedbackRepository
    {
        Task<CusCreateFeedbackVM?> GetCusCreateFeedbackAsync(int customerId, int appointmentId);
        Task<bool> HasFeedbackAsync(int customerId, int appointmentId);
        Task<int> CreateFeedbackAsync(int customerId, CusCreateFeedbackVM vm);

        Task<PagedResult<CusFeedbackHistoryItemVM>> GetCusFeedbackHistoryAsync(int customerId, CusFeedbackHistoryQuery query);
    

        Task<PagedResult<Feedback>> GetPagedAsync(string? search, int? starFilter, int page, int pageSize);
        Task<List<Feedback>> GetTopFeedbacksAsync(int count);
        Task<Feedback?> GetByIdAsync(int id);
    }
}
