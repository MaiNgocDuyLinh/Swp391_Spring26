using Group3_SWP391_PetMedical.Models;
using Group3_SWP391_PetMedical.Models.Common;

namespace Group3_SWP391_PetMedical.Repository.Interfaces
{
    public interface IAppointmentRepository
    {
        Task<PagedResult<Appointment>> GetAppointmentsByDatePagedAsync(DateTime date, string? search, int page, int pageSize);

        Task<PagedResult<Appointment>> GetAllAppointmentsPagedAsync(string? search, string? statusFilter, int page, int pageSize);

        Task<Appointment?> GetByIdAsync(int id);

        Task<bool> ApproveAsync(int id);

        Task<bool> RejectAsync(int id, string? reason);

        Task<bool> AssignDoctorAsync(int appointmentId, int doctorId);

        Task<List<User>> GetDoctorsAsync();
    }
}
