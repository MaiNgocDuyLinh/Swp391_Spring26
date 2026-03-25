using Group3_SWP391_PetMedical.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Group3_SWP391_PetMedical.Services.Interfaces
{
    public interface IRetailOrderService
    {
        Task<IEnumerable<RetailOrder>> GetOrdersByUserIdAsync(int userId, string? status = null);
        Task<IEnumerable<RetailOrder>> GetAllOrdersAsync(DateTime? date, string? search, string? status, string? statusOrder);
        Task<RetailOrder?> GetOrderByIdAsync(int id);
        Task UpdateStatusOrderAsync(int orderId, string statusOrder);
        Task<bool> CancelOrderAsync(int orderId, int userId);
    }
}
