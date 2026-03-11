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

        // ========== Appointments ==========
        Task<PagedResult<Appointment>> GetAllAppointmentsPagedAsync(string? search, string? statusFilter, int page, int pageSize);
        Task<Appointment?> GetAppointmentByIdAsync(int id);
        Task<bool> AssignDoctorAsync(int appointmentId, int doctorId);
        Task<List<User>> GetDoctorsAsync();
    }
}
