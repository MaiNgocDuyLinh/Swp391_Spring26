using Group3_SWP391_PetMedical.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Group3_SWP391_PetMedical.Repository.Interfaces
{
    public interface IRetailOrderRepository
    {
        Task<IEnumerable<RetailOrder>> GetOrdersByUserIdAsync(int userId);
        Task<IEnumerable<RetailOrder>> GetAllOrdersAsync(DateTime? date, string? search, string? status);
        Task<RetailOrder?> GetOrderByIdAsync(int id);
    }
}
