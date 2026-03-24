using Group3_SWP391_PetMedical.Models;
using Group3_SWP391_PetMedical.Models.Common;
using Group3_SWP391_PetMedical.Repository.Interfaces;
using Group3_SWP391_PetMedical.Services.Interfaces;

namespace Group3_SWP391_PetMedical.Services.Implementations
{
    public class StaffService : IStaffService
    {
        private readonly IServiceRepository _serviceRepo;
        private readonly IUserRepository _userRepo;
        private readonly IAppointmentRepository _appointmentRepo;

        public StaffService(
            IServiceRepository serviceRepo,
            IUserRepository userRepo,
            IAppointmentRepository appointmentRepo)
        {
            _serviceRepo = serviceRepo;
            _userRepo = userRepo;
            _appointmentRepo = appointmentRepo;
        }

        // ========== Services (view only) ==========
        public Task<PagedResult<Service>> GetServicesPagedAsync(string? search, int page, int pageSize)
            => _serviceRepo.GetPagedAsync(search, page, pageSize);

        // ========== Customers ==========
        public Task<PagedResult<User>> GetCustomersPagedAsync(string? search, int page, int pageSize)
            => _userRepo.GetCustomersPagedAsync(search, page, pageSize);

        public Task<User?> GetCustomerDetailAsync(int customerId)
            => _userRepo.GetCustomerDetailAsync(customerId);

        // ========== Appointments ==========
        public Task<PagedResult<Appointment>> GetAppointmentsByDatePagedAsync(DateTime date, string? search, int page, int pageSize)
            => _appointmentRepo.GetAppointmentsByDatePagedAsync(date, search, page, pageSize);

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

        public async Task<(bool Success, bool IsEarlyArrival)> UpdateAppointmentStatusAsync(int id, string newStatus)
        {
            var appt = await _appointmentRepo.GetByIdAsync(id);
            if (appt == null) return (false, false);

            bool isEarly = (newStatus == "Đã đến" && appt.appointment_date.Date > DateTime.Today);
            bool success = await _appointmentRepo.UpdateStatusAsync(id, newStatus, isEarly);
            return (success, isEarly);
        }

        // ========== Invoice ==========
        public Task<Invoice?> GetInvoiceByAppointmentIdAsync(int appointmentId)
            => _appointmentRepo.GetInvoiceByAppointmentIdAsync(appointmentId);

        public Task<bool> CreateInvoiceAsync(int appointmentId)
            => _appointmentRepo.CreateInvoiceAsync(appointmentId);
    }
}
