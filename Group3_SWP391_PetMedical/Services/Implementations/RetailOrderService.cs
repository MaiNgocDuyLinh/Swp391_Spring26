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

        public async Task<IEnumerable<RetailOrder>> GetOrdersByUserIdAsync(int userId)
        {
            return await _retailOrderRepo.GetOrdersByUserIdAsync(userId);
        }

        public async Task<IEnumerable<RetailOrder>> GetAllOrdersAsync(DateTime? date, string? search, string? status)
        {
            return await _retailOrderRepo.GetAllOrdersAsync(date, search, status);
        }

        public async Task<RetailOrder?> GetOrderByIdAsync(int id)
        {
            return await _retailOrderRepo.GetOrderByIdAsync(id);
        }

        public async Task UpdateStatusOrderAsync(int orderId, string statusOrder)
        {
            await _retailOrderRepo.UpdateStatusOrderAsync(orderId, statusOrder);
        }
    }
}
