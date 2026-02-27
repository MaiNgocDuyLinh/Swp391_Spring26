using Group3_SWP391_PetMedical.Models.Common;
using Group3_SWP391_PetMedical.Repository.Interfaces;
using Group3_SWP391_PetMedical.Services.Interfaces;
using Group3_SWP391_PetMedical.ViewModels.Appointment;

namespace Group3_SWP391_PetMedical.Services.Implementations
{
    public class CusAppointmentService : ICusAppointmentService
    {
        private readonly ICusAppointmentRepository _repo;

        public CusAppointmentService(ICusAppointmentRepository repo)
        {
            _repo = repo;
        }

        public Task<PagedResult<CusAppointmentHistoryItemVM>>
            GetCusAppointmentHistoryAsync(int customerId, CusAppointmentHistoryQuery query)
            => _repo.GetCusAppointmentHistoryAsync(customerId, query);
    }
}