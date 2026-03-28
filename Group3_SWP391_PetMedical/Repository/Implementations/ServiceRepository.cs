using Group3_SWP391_PetMedical.Models;
using Group3_SWP391_PetMedical.Models.Common;
using Group3_SWP391_PetMedical.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Group3_SWP391_PetMedical.Repository.Implementations
{
    public class ServiceRepository : IServiceRepository
    {
        private readonly PetClinicContext _context;

        public ServiceRepository(PetClinicContext context)
        {
            _context = context;
        }


        public Task<List<Service>> GetAllAsync()
           => _context.Services
           .Where(s => s.status == true)
         .Include(s => s.ServiceDiscounts.Where(sd => sd.is_active == true))
         .AsNoTracking()
        .OrderBy(s => s.service_name)
        .ToListAsync();

        // Xem danh sách dịch vụ (Staff + Manager dùng chung)
        public async Task<PagedResult<Service>> GetPagedAsync(string? search, int page, int pageSize)
        {
            var query = _context.Services
                .Where(s => s.status == true)
                .Include(s => s.ServiceDiscounts.Where(sd => sd.is_active == true))
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();
                query = query.Where(s =>
                    EF.Functions.Collate(s.service_name, "Vietnamese_CI_AI").Contains(search) ||
                    EF.Functions.Collate(s.description ?? "", "Vietnamese_CI_AI").Contains(search));
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

        // Lấy chi tiết dịch vụ (Manager dùng để edit)
        public async Task<Service?> GetByIdAsync(int id)
        {
            return await _context.Services.FirstOrDefaultAsync(s => s.service_id == id);
        }

        // Cập nhật toàn bộ thông tin dịch vụ (Manager only)
        public async Task<bool> UpdateAsync(int id, string serviceName, decimal basePrice, string? description, int? duration, bool isHomeService, bool status)
        {
            var service = await _context.Services.FirstOrDefaultAsync(s => s.service_id == id);
            if (service == null) return false;

            service.service_name = serviceName;
            service.base_price = basePrice;
            service.description = description;
            service.duration = duration;
            service.is_home_service = isHomeService;
            service.status = status;

            await _context.SaveChangesAsync();
            return true;
        }

        // Thêm dịch vụ mới
        public async Task<bool> CreateAsync(string serviceName, decimal basePrice, string? description, int? duration, bool isHomeService)
        {
            var service = new Service
            {
                service_name = serviceName,
                base_price = basePrice,
                description = description,
                duration = duration,
                is_home_service = isHomeService,
                status = true
            };
            _context.Services.Add(service);
            await _context.SaveChangesAsync();
            return true;
        }

        // Xóa dịch vụ
        public async Task<bool> DeleteAsync(int id)
        {
            var service = await _context.Services.FirstOrDefaultAsync(s => s.service_id == id);
            if (service == null) return false;

            // Check if service is linked to any appointment
            var hasAppointments = await _context.AppointmentDetails.AnyAsync(ad => ad.service_id == id);
            if (hasAppointments) return false;

            _context.Services.Remove(service);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsByNameAsync(string name, int? excludeId = null)
        {
            var query = _context.Services.AsNoTracking().AsQueryable();
            if (excludeId.HasValue)
            {
                query = query.Where(s => s.service_id != excludeId.Value);
            }
            return await query.AnyAsync(s => s.service_name.ToLower() == name.ToLower());
        }
    }
}
