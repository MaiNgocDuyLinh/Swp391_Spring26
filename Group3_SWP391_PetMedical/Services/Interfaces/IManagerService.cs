using Group3_SWP391_PetMedical.Models;
using Group3_SWP391_PetMedical.Models.Common;

namespace Group3_SWP391_PetMedical.Services.Interfaces
{
    public interface IManagerService
    {
        // ========== Services ==========
        Task<PagedResult<Service>> GetServicesPagedAsync(string? search, int page, int pageSize);
        Task<Service?> GetServiceByIdAsync(int id);
        Task<bool> UpdateServiceAsync(int id, string serviceName, decimal basePrice, string? description, int? duration, bool isHomeService, bool status);
        Task<bool> CreateServiceAsync(string serviceName, decimal basePrice, string? description, int? duration, bool isHomeService);
        Task<bool> DeleteServiceAsync(int id);

        // ========== Appointments ==========
        Task<PagedResult<Appointment>> GetAllAppointmentsPagedAsync(string? search, string? statusFilter, int page, int pageSize);
        Task<PagedResult<Appointment>> GetCancelledAppointmentsPagedAsync(string? search, int page, int pageSize);
        Task<Appointment?> GetAppointmentByIdAsync(int id);
        Task<bool> CancelAppointmentAsync(int id, string? reason);
        Task<bool> AssignDoctorAsync(int appointmentId, int doctorId);
        Task<List<User>> GetDoctorsAsync();
    }
}
