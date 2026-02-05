using Group3_SWP391_PetMedical.Models;
using Group3_SWP391_PetMedical.Models.Common;

namespace Group3_SWP391_PetMedical.Services.Interfaces
{
    public interface IStaffService
    {
        // Services (view only - Staff chỉ xem, không sửa)
        Task<PagedResult<Service>> GetServicesPagedAsync(string? search, int page, int pageSize);

        // Customers
        Task<PagedResult<User>> GetCustomersPagedAsync(string? search, int page, int pageSize);

        // Appointments
        Task<PagedResult<Appointment>> GetAppointmentsByDatePagedAsync(DateTime date, string? search, int page, int pageSize);
    }
}
