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

        public async Task<IEnumerable<RetailOrder>> GetAllOrdersAsync(DateTime? date, string? search, string? status, string? statusOrder)
        {
            var query = _context.RetailOrders
                .Include(o => o.user)
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.medicine)
                .AsQueryable();

            if (date.HasValue)
            {
                query = query.Where(o => o.pickup_date.HasValue && o.pickup_date.Value.Date == date.Value.Date);
            }

            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(o => o.status == status);
            }

            if (!string.IsNullOrEmpty(statusOrder))
            {
                query = query.Where(o => o.status_order == statusOrder);
            }

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(o => (o.user != null && o.user.full_name != null && o.user.full_name.Contains(search))
                                      || (o.id.ToString().Contains(search))
                                      || (o.note != null && o.note.Contains(search))
                                      || (o.status_order != null && o.status_order.Contains(search)));
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

        public async Task UpdateStatusOrderAsync(int orderId, string statusOrder)
        {
            var order = await _context.RetailOrders.FindAsync(orderId);
            if (order != null)
            {
                order.status_order = statusOrder;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> CancelAndReturnStockAsync(int orderId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var order = await _context.RetailOrders
                    .Include(o => o.OrderDetails)
                        .ThenInclude(od => od.medicine)
                    .FirstOrDefaultAsync(o => o.id == orderId);

                if (order == null) return false;

                // Update status
                order.status_order = "Hủy/Hoàn trả";

                // Return stock
                foreach (var detail in order.OrderDetails)
                {
                    if (detail.medicine != null)
                    {
                        detail.medicine.stock_quantity = (detail.medicine.stock_quantity ?? 0) + detail.quantity;
                    }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                return false;
            }
        }
    }
}
