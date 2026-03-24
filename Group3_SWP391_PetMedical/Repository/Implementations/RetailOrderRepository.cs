using Group3_SWP391_PetMedical.Models;
using Group3_SWP391_PetMedical.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Group3_SWP391_PetMedical.Repository.Implementations
{
    public class RetailOrderRepository : IRetailOrderRepository
    {
        private readonly PetClinicContext _context;

        public RetailOrderRepository(PetClinicContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<RetailOrder>> GetOrdersByUserIdAsync(int userId)
        {
            return await _context.RetailOrders
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.medicine)
                .Where(o => o.user_id == userId)
                .OrderByDescending(o => o.created_at)
                .ToListAsync();
        }

        public async Task<IEnumerable<RetailOrder>> GetAllOrdersAsync(DateTime? date, string? search, string? status)
        {
            var query = _context.RetailOrders
                .Include(o => o.user)
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.medicine)
                .AsQueryable();

            if (date.HasValue)
            {
                query = query.Where(o => o.created_at.HasValue && o.created_at.Value.Date == date.Value.Date);
            }

            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(o => o.status == status);
            }

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(o => (o.user != null && o.user.full_name != null && o.user.full_name.Contains(search))
                                      || (o.note != null && o.note.Contains(search)));
            }

            return await query.OrderByDescending(o => o.created_at).ToListAsync();
        }

        public async Task<RetailOrder?> GetOrderByIdAsync(int id)
        {
            return await _context.RetailOrders
                .Include(o => o.user)
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.medicine)
                .FirstOrDefaultAsync(o => o.id == id);
        }
    }
}
