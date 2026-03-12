using Group3_SWP391_PetMedical.Models.Common;
using Group3_SWP391_PetMedical.Repository.Interfaces;
using Group3_SWP391_PetMedical.Services.Interfaces;
using Group3_SWP391_PetMedical.ViewModels.Feedback;

namespace Group3_SWP391_PetMedical.Services.Implementations
{
    public class FeedbackService : IFeedbackService
    {
        private readonly IFeedbackRepository _repo;

        public FeedbackService(IFeedbackRepository repo)
        {
            _repo = repo;
        }

        public Task<CusCreateFeedbackVM?> GetCusCreateFeedbackAsync(int customerId, int appointmentId)
            => _repo.GetCusCreateFeedbackAsync(customerId, appointmentId);

        public Task<bool> HasFeedbackAsync(int customerId, int appointmentId)
            => _repo.HasFeedbackAsync(customerId, appointmentId);

        public async Task<int> CreateFeedbackAsync(int customerId, CusCreateFeedbackVM vm)
        {
            if (vm == null) throw new Exception("Dữ liệu feedback không hợp lệ.");

            if (!vm.Rating.HasValue)
                throw new Exception("Vui lòng chọn số sao.");

            if (vm.Rating < 1 || vm.Rating > 5)
                throw new Exception("Số sao phải từ 1 đến 5.");

            return await _repo.CreateFeedbackAsync(customerId, vm);
        }
        public Task<PagedResult<CusFeedbackHistoryItemVM>> GetCusFeedbackHistoryAsync(int customerId, CusFeedbackHistoryQuery query)
    => _repo.GetCusFeedbackHistoryAsync(customerId, query);
    }
}