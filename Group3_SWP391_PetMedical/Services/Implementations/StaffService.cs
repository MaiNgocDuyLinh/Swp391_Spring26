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

        // ========== SERVICES (view only) ==========
        public Task<PagedResult<Service>> GetServicesPagedAsync(string? search, int page, int pageSize)
        {
            return _serviceRepo.GetPagedAsync(search, page, pageSize);
        }

        // ========== CUSTOMERS ==========
        public Task<PagedResult<User>> GetCustomersPagedAsync(string? search, int page, int pageSize)
        {
            return _userRepo.GetCustomersPagedAsync(search, page, pageSize);
        }

        // ========== APPOINTMENTS ==========
        public Task<PagedResult<Appointment>> GetAppointmentsByDatePagedAsync(DateTime date, string? search, int page, int pageSize)
        {
            return _appointmentRepo.GetAppointmentsByDatePagedAsync(date, search, page, pageSize);
        }
    }
}
