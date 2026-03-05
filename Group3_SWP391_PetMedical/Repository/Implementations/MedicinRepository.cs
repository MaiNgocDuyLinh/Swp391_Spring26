using Group3_SWP391_PetMedical.Models;
using Group3_SWP391_PetMedical.Models.Common;
using Group3_SWP391_PetMedical.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Group3_SWP391_PetMedical.Repository.Implementations
{
    public class MedicinRepository : IMedicinRepository
    {
        private readonly PetClinicContext _context;

        public MedicinRepository(PetClinicContext context)
        {
            _context = context;
        }

        public async Task<PagedResult<Medication>> GetPagedAsync(string? search, int page, int pageSize)
        {
            var query = _context.Medications.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();
                query = query.Where(m =>
                    m.name.ToLower().Contains(keyword));
            }

            var totalItems = await query.CountAsync();

            var items = await query
                .OrderBy(m => m.name)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<Medication>
            {
                Items = items,
                Page = page,
                PageSize = pageSize,
                TotalItems = totalItems
            };
        }

        public Task<Medication?> GetByIdAsync(int id)
            => _context.Medications.AsNoTracking()
                .FirstOrDefaultAsync(m => m.medicine_id == id);

        public async Task<Medication> AddAsync(string name, decimal unitPrice, int stockQuantity, string? description)
        {
            var entity = new Medication
            {
                name = name,
                unit_price = unitPrice,
                stock_quantity = stockQuantity,
                description = description
            };

            _context.Medications.Add(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task<bool> UpdateAsync(int id, string name, decimal unitPrice, int stockQuantity, string? description)
        {
            var entity = await _context.Medications.FirstOrDefaultAsync(m => m.medicine_id == id);
            if (entity == null) return false;

            entity.name = name;
            entity.unit_price = unitPrice;
            entity.stock_quantity = stockQuantity;
            entity.description = description;

            await _context.SaveChangesAsync();
            return true;
        }
    }
}
