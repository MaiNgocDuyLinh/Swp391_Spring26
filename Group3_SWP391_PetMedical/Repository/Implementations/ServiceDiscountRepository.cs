using Group3_SWP391_PetMedical.Models;
using Group3_SWP391_PetMedical.Models.Common;
using Group3_SWP391_PetMedical.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Group3_SWP391_PetMedical.Repository.Implementations
{
    public class ServiceDiscountRepository : IServiceDiscountRepository
    {
        private readonly PetClinicContext _context;

        public ServiceDiscountRepository(PetClinicContext context)
        {
            _context = context;
        }

        public async Task<PagedResult<Service>> GetServicesWithDiscountPagedAsync(string? search, int page, int pageSize)
        {
            var query = _context.Services
                .Include(s => s.ServiceDiscounts.Where(sd => sd.is_active == true))
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();
                query = query.Where(s =>
                    EF.Functions.Collate(s.service_name, "Vietnamese_CI_AI").Contains(search));
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

        public async Task<ServiceDiscount?> GetActiveDiscountByServiceIdAsync(int serviceId)
        {
            return await _context.ServiceDiscounts
                .Include(sd => sd.service)
                .FirstOrDefaultAsync(sd => sd.service_id == serviceId && sd.is_active == true);
        }

        public async Task<bool> ApplyDiscountAsync(int serviceId, int discountPercent, DateTime startDate, DateTime endDate)
        {
            // Kiểm tra service có tồn tại không
            var service = await _context.Services.FirstOrDefaultAsync(s => s.service_id == serviceId);
            if (service == null) return false;

            // Hủy giảm giá cũ nếu có
            var existingDiscount = await _context.ServiceDiscounts
                .FirstOrDefaultAsync(sd => sd.service_id == serviceId && sd.is_active == true);

            if (existingDiscount != null)
            {
                existingDiscount.is_active = false;
            }

            // Tạo giảm giá mới
            var discount = new ServiceDiscount
            {
                service_id = serviceId,
                discount_percent = discountPercent,
                start_date = startDate,
                end_date = endDate,
                is_active = true,
                created_at = DateTime.Now
            };

            _context.ServiceDiscounts.Add(discount);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RemoveDiscountAsync(int discountId)
        {
            var discount = await _context.ServiceDiscounts.FirstOrDefaultAsync(sd => sd.discount_id == discountId);
            if (discount == null) return false;

            discount.is_active = false;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<PagedResult<ServiceDiscount>> GetDiscountHistoryPagedAsync(string? search, int page, int pageSize)
        {
            var query = _context.ServiceDiscounts
                .Include(sd => sd.service)
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();
                query = query.Where(sd =>
                    EF.Functions.Collate(sd.service.service_name, "Vietnamese_CI_AI").Contains(search));
            }

            var total = await query.CountAsync();
            var items = await query
                .OrderByDescending(sd => sd.created_at)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<ServiceDiscount>
            {
                Items = items,
                Page = page,
                PageSize = pageSize,
                TotalItems = total
            };
        }
    }
}
