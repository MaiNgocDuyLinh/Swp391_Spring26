using Group3_SWP391_PetMedical.Models;
using Group3_SWP391_PetMedical.Models.Common;
using Group3_SWP391_PetMedical.Repository.Interfaces;
using Group3_SWP391_PetMedical.Services.Interfaces;

namespace Group3_SWP391_PetMedical.Services.Implementations
{
    public class ManagerService : IManagerService
    {
        private readonly IServiceRepository _serviceRepo;
        private readonly IAppointmentRepository _appointmentRepo;
        private readonly IFeedbackRepository _feedbackRepo;
        private readonly IServiceDiscountRepository _discountRepo;

        public ManagerService(
            IServiceRepository serviceRepo,
            IAppointmentRepository appointmentRepo,
            IFeedbackRepository feedbackRepo,
            IServiceDiscountRepository discountRepo)
        {
            _serviceRepo = serviceRepo;
            _appointmentRepo = appointmentRepo;
            _feedbackRepo = feedbackRepo;
            _discountRepo = discountRepo;
        }

        // ========== Services ==========
        public Task<PagedResult<Service>> GetServicesPagedAsync(string? search, int page, int pageSize)
            => _serviceRepo.GetPagedAsync(search, page, pageSize);

        public Task<Service?> GetServiceByIdAsync(int id)
            => _serviceRepo.GetByIdAsync(id);

        public Task<bool> UpdateServiceAsync(int id, string serviceName, decimal basePrice, string? description, int? duration, bool isHomeService, bool status)
            => _serviceRepo.UpdateAsync(id, serviceName, basePrice, description, duration, isHomeService, status);

        public Task<bool> CreateServiceAsync(string serviceName, decimal basePrice, string? description, int? duration, bool isHomeService)
            => _serviceRepo.CreateAsync(serviceName, basePrice, description, duration, isHomeService);

        public Task<bool> DeleteServiceAsync(int id)
            => _serviceRepo.DeleteAsync(id);

        public Task<bool> ServiceNameExistsAsync(string name, int? excludeId = null)
            => _serviceRepo.ExistsByNameAsync(name, excludeId);

        // ========== Appointments ==========
        public Task<PagedResult<Appointment>> GetAllAppointmentsPagedAsync(string? search, string? statusFilter, int page, int pageSize)
            => _appointmentRepo.GetAllAppointmentsPagedAsync(search, statusFilter, page, pageSize);

        public Task<PagedResult<Appointment>> GetCancelledAppointmentsPagedAsync(string? search, int page, int pageSize)
            => _appointmentRepo.GetCancelledAppointmentsPagedAsync(search, page, pageSize);

        public Task<Appointment?> GetAppointmentByIdAsync(int id)
            => _appointmentRepo.GetByIdAsync(id);

        public Task<bool> CancelAppointmentAsync(int id, string? reason)
            => _appointmentRepo.CancelAsync(id, reason);

        public Task<bool> AssignDoctorAsync(int appointmentId, int doctorId)
            => _appointmentRepo.AssignDoctorAsync(appointmentId, doctorId);

        public Task<List<User>> GetDoctorsAsync()
            => _appointmentRepo.GetDoctorsAsync();

        // ========== Feedbacks ==========
        public Task<PagedResult<Feedback>> GetFeedbacksPagedAsync(string? search, int? starFilter, int page, int pageSize)
            => _feedbackRepo.GetPagedAsync(search, starFilter, page, pageSize);

        public Task<Feedback?> GetFeedbackByIdAsync(int id)
            => _feedbackRepo.GetByIdAsync(id);

        // ========== Service Discounts ==========
        public Task<PagedResult<Service>> GetServicesWithDiscountPagedAsync(string? search, int page, int pageSize)
            => _discountRepo.GetServicesWithDiscountPagedAsync(search, page, pageSize);

        public Task<ServiceDiscount?> GetActiveDiscountByServiceIdAsync(int serviceId)
            => _discountRepo.GetActiveDiscountByServiceIdAsync(serviceId);

        public Task<bool> ApplyDiscountAsync(int serviceId, int discountPercent, DateTime startDate, DateTime endDate)
            => _discountRepo.ApplyDiscountAsync(serviceId, discountPercent, startDate, endDate);

        public Task<bool> RemoveDiscountAsync(int discountId)
            => _discountRepo.RemoveDiscountAsync(discountId);

        public Task<PagedResult<ServiceDiscount>> GetDiscountHistoryPagedAsync(string? search, int page, int pageSize)
            => _discountRepo.GetDiscountHistoryPagedAsync(search, page, pageSize);
    }
}
