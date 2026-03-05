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
                    m.name.ToLower().Contains(keyword) ||
                    (m.description ?? "").ToLower().Contains(keyword));
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
    }
}
