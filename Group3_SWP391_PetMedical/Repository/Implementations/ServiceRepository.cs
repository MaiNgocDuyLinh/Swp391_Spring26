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

        // Xem danh sách dịch vụ (Staff + Manager dùng chung)
        public async Task<PagedResult<Service>> GetPagedAsync(string? search, int page, int pageSize)
        {
            var query = _context.Services.AsNoTracking().AsQueryable();

            // Tìm kiếm theo tên hoặc mô tả
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

        // Lấy chi tiết dịch vụ (Manager dùng để edit)
        public async Task<Service?> GetByIdAsync(int id)
        {
            return await _context.Services.FirstOrDefaultAsync(s => s.service_id == id);
        }

        // Cập nhật dịch vụ (Manager only)
        public async Task<bool> UpdateAsync(int id, decimal basePrice, string? description)
        {
            var service = await _context.Services.FirstOrDefaultAsync(s => s.service_id == id);
            if (service == null) return false;

            service.base_price = basePrice;
            service.description = description;

            await _context.SaveChangesAsync();
            return true;
        }
    }
}
