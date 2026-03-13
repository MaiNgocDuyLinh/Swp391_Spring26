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

        public ManagerService(IServiceRepository serviceRepo, IAppointmentRepository appointmentRepo)
        {
            _serviceRepo = serviceRepo;
            _appointmentRepo = appointmentRepo;
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
    }
}
