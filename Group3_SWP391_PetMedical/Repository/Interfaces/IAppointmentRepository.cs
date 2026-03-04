using Group3_SWP391_PetMedical.Models;
using Group3_SWP391_PetMedical.Models.Common;

namespace Group3_SWP391_PetMedical.Repository.Interfaces
{
    public interface IAppointmentRepository
    {
        // ========== Staff: Xem lịch theo ngày ==========
        Task<PagedResult<Appointment>> GetAppointmentsByDatePagedAsync(DateTime date, string? search, int page, int pageSize);

        // ========== Manager + Staff: Xem tất cả lịch (có filter status) ==========
        Task<PagedResult<Appointment>> GetAllAppointmentsPagedAsync(string? search, string? statusFilter, int page, int pageSize);

        // ========== Get chi tiết 1 lịch ==========
        Task<Appointment?> GetByIdAsync(int id);

        // ========== Manager: Approve / Reject ==========
        Task<bool> ApproveAsync(int id);
        Task<bool> RejectAsync(int id, string? reason);

        // ========== Manager + Staff: Assign Doctor ==========
        Task<bool> AssignDoctorAsync(int appointmentId, int doctorId);
        Task<List<User>> GetDoctorsAsync();

        // ========== Staff: Update status ==========
        Task<bool> UpdateStatusAsync(int id, string newStatus);

        // ========== Staff: View Invoice ==========
        Task<Invoice?> GetInvoiceByAppointmentIdAsync(int appointmentId);
        Task<bool> CreateInvoiceAsync(int appointmentId);
    }
}