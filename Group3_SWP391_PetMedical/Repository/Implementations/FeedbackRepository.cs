using Group3_SWP391_PetMedical.Models;
using Group3_SWP391_PetMedical.Models.Common;
using Group3_SWP391_PetMedical.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Group3_SWP391_PetMedical.Repository.Implementations
{
    public class FeedbackRepository : IFeedbackRepository
    {
        private readonly PetClinicContext _context;

        public FeedbackRepository(PetClinicContext context)
        {
            _context = context;
        }

        public async Task<PagedResult<Feedback>> GetPagedAsync(string? search, int? starFilter, int page, int pageSize)
        {
            var query = _context.Feedback
                .Include(f => f.customer)
                .Include(f => f.appointment)
                .ThenInclude(a => a.doctor)
                .Include(f => f.appointment)
                .ThenInclude(a => a.pet)
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();
                query = query.Where(f => 
                    EF.Functions.Collate(f.comment ?? "", "Vietnamese_CI_AI").Contains(search) ||
                    EF.Functions.Collate(f.customer.full_name ?? "", "Vietnamese_CI_AI").Contains(search) ||
                    EF.Functions.Collate(f.appointment.pet.name ?? "", "Vietnamese_CI_AI").Contains(search)
                );
            }

            if (starFilter.HasValue && starFilter > 0)
            {
                query = query.Where(f => f.rating == starFilter.Value);
            }

            var total = await query.CountAsync();
            var items = await query
                .OrderByDescending(f => f.created_at)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<Feedback>
            {
                Items = items,
                Page = page,
                PageSize = pageSize,
                TotalItems = total
            };
        }

        public async Task<List<Feedback>> GetTopFeedbacksAsync(int count)
        {
            return await _context.Feedback
                .Include(f => f.customer)
                .Where(f => f.rating >= 4 && !string.IsNullOrWhiteSpace(f.comment)) // Only good reviews with comments
                .OrderByDescending(f => f.created_at)
                .Take(count)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<Feedback?> GetByIdAsync(int id)
        {
            return await _context.Feedback
                .Include(f => f.customer)
                .Include(f => f.appointment)
                .ThenInclude(a => a.doctor)
                .Include(f => f.appointment)
                .ThenInclude(a => a.pet)
                .Include(f => f.appointment)
                .ThenInclude(a => a.AppointmentDetails)
                .ThenInclude(d => d.service)
                .AsNoTracking()
                .FirstOrDefaultAsync(f => f.feedback_id == id);
        }
    }
}
