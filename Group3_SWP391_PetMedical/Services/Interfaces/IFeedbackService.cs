using Group3_SWP391_PetMedical.Models.Common;
using Group3_SWP391_PetMedical.ViewModels.Feedback;

namespace Group3_SWP391_PetMedical.Services.Interfaces
{
    public interface IFeedbackService
    {
        Task<CusCreateFeedbackVM?> GetCusCreateFeedbackAsync(int customerId, int appointmentId);
        Task<bool> HasFeedbackAsync(int customerId, int appointmentId);
        Task<int> CreateFeedbackAsync(int customerId, CusCreateFeedbackVM vm);

        Task<PagedResult<CusFeedbackHistoryItemVM>> GetCusFeedbackHistoryAsync(int customerId, CusFeedbackHistoryQuery query);
    }
}