using Group3_SWP391_PetMedical.Models;
using Group3_SWP391_PetMedical.Models.Common;
using Group3_SWP391_PetMedical.Repository.Interfaces;
using Group3_SWP391_PetMedical.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Group3_SWP391_PetMedical.Services.Implementations
{
    public class StaffService : IStaffService
    {
        private readonly IUserRepository _userRepo;
        private readonly IAppointmentRepository _appointmentRepo;
        private readonly PetClinicContext _context;

        public StaffService(
            IUserRepository userRepo,
            IAppointmentRepository appointmentRepo,
            PetClinicContext context)
        {
            _userRepo = userRepo;
            _appointmentRepo = appointmentRepo;
            _context = context;
        }

        // ========== SERVICES ==========
        public async Task<PagedResult<Service>> GetServicesPagedAsync(string? search, int page, int pageSize)
        {
            var query = _context.Services.AsNoTracking().AsQueryable();
            
            // Tìm kiếm theo tên dịch vụ hoặc mô tả
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim().ToLower();
                query = query.Where(s => 
                    s.service_name.ToLower().Contains(search) ||
                    (s.description ?? "").ToLower().Contains(search));
            }

            var total = await query.CountAsync();
            var items = await query
                .OrderBy(s => s.service_name)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<Service>
            {
                Items = items,
                Page = page,
                PageSize = pageSize,
                TotalItems = total
            };
        }

        public async Task<Service?> GetServiceByIdAsync(int id)
        {
            return await _context.Services.FirstOrDefaultAsync(s => s.service_id == id);
        }

        public async Task<bool> UpdateServiceAsync(int id, decimal basePrice, string? description)
        {
            var service = await _context.Services.FirstOrDefaultAsync(s => s.service_id == id);
            if (service == null) return false;

            service.base_price = basePrice;
            service.description = description;

            await _context.SaveChangesAsync();
            return true;
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
