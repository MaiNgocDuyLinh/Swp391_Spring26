using Group3_SWP391_PetMedical.Models;
using Group3_SWP391_PetMedical.Models.Common;

namespace Group3_SWP391_PetMedical.Services.Interfaces
{
    public interface IStaffService
    {
        // ========== Services (view only) ==========
        Task<PagedResult<Service>> GetServicesPagedAsync(string? search, int page, int pageSize);

        // ========== Customers ==========
        Task<PagedResult<User>> GetCustomersPagedAsync(string? search, int page, int pageSize);
        Task<User?> GetCustomerDetailAsync(int customerId);

        // ========== Appointments ==========
        Task<PagedResult<Appointment>> GetAppointmentsByDatePagedAsync(DateTime date, string? search, int page, int pageSize);
        Task<PagedResult<Appointment>> GetAllAppointmentsPagedAsync(string? search, string? statusFilter, int page, int pageSize);
        Task<PagedResult<Appointment>> GetCancelledAppointmentsPagedAsync(string? search, int page, int pageSize);
        Task<Appointment?> GetAppointmentByIdAsync(int id);
        Task<bool> CancelAppointmentAsync(int id, string? reason);
        Task<bool> AssignDoctorAsync(int appointmentId, int doctorId);
        Task<List<User>> GetDoctorsAsync();
        Task<(bool Success, bool IsEarlyArrival)> UpdateAppointmentStatusAsync(int id, string newStatus);

        // ========== Invoice ==========
        Task<Invoice?> GetInvoiceByAppointmentIdAsync(int appointmentId);
        Task<bool> CreateInvoiceAsync(int appointmentId);
    }
}
