using Group3_SWP391_PetMedical.Models;
using Group3_SWP391_PetMedical.Repository.Interfaces;
using Group3_SWP391_PetMedical.Services.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Group3_SWP391_PetMedical.Services.Implementations
{
    public class RetailOrderService : IRetailOrderService
    {
        private readonly IRetailOrderRepository _retailOrderRepo;

        public RetailOrderService(IRetailOrderRepository retailOrderRepo)
        {
            _retailOrderRepo = retailOrderRepo;
        }

        public async Task<IEnumerable<RetailOrder>> GetOrdersByUserIdAsync(int userId, string? status = null)
        {
            return await _retailOrderRepo.GetOrdersByUserIdAsync(userId, status);
        }

        public async Task<IEnumerable<RetailOrder>> GetAllOrdersAsync(DateTime? date, string? search, string? status, string? statusOrder)
        {
            return await _retailOrderRepo.GetAllOrdersAsync(date, search, status, statusOrder);
        }

        public async Task<RetailOrder?> GetOrderByIdAsync(int id)
        {
            return await _retailOrderRepo.GetOrderByIdAsync(id);
        }

        public async Task UpdateStatusOrderAsync(int orderId, string statusOrder)
        {
            await _retailOrderRepo.UpdateStatusOrderAsync(orderId, statusOrder);
        }

        public async Task<bool> CancelOrderAsync(int orderId, int userId)
        {
            var order = await _retailOrderRepo.GetOrderByIdAsync(orderId);
            if (order == null || order.user_id != userId) return false;


            if (order.status_order != "Đã tiếp nhận") return false;

            // CancelAndReturnStockAsync will handle both status change and stock return

            
            return await _retailOrderRepo.CancelAndReturnStockAsync(orderId);
        }
    }
}
