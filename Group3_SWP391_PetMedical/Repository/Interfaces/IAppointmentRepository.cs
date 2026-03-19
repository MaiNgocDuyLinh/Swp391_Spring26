using Group3_SWP391_PetMedical.Models;
using Group3_SWP391_PetMedical.Models.Common;

namespace Group3_SWP391_PetMedical.Repository.Interfaces
{
    public interface IAppointmentRepository
    {
        // ========== View appointments ==========
        Task<PagedResult<Appointment>> GetAppointmentsByDatePagedAsync(DateTime date, string? search, int page, int pageSize);
        Task<PagedResult<Appointment>> GetAllAppointmentsPagedAsync(string? search, string? statusFilter, int page, int pageSize);
        Task<PagedResult<Appointment>> GetCancelledAppointmentsPagedAsync(string? search, int page, int pageSize);
        Task<Appointment?> GetByIdAsync(int id);

        // ========== Cancel ==========
        Task<bool> CancelAsync(int id, string? reason);

        // ========== Assign Doctor ==========
        Task<bool> AssignDoctorAsync(int appointmentId, int doctorId);
        Task<List<User>> GetDoctorsAsync();

        // ========== Staff: Update status ==========
        Task<bool> UpdateStatusAsync(int id, string newStatus);

        // ========== Invoice ==========
        Task<Invoice?> GetInvoiceByAppointmentIdAsync(int appointmentId);
        Task<bool> CreateInvoiceAsync(int appointmentId);
    }
}