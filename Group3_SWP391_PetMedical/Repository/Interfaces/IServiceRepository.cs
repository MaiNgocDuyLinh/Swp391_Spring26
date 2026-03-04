using Group3_SWP391_PetMedical.Models;
using Group3_SWP391_PetMedical.Models.Common;

namespace Group3_SWP391_PetMedical.Repository.Interfaces
{
    public interface IServiceRepository
    {
        // Xem danh sách dịch vụ (Staff + Manager dùng chung)
        Task<PagedResult<Service>> GetPagedAsync(string? search, int page, int pageSize);
        
        // Lấy chi tiết dịch vụ (Manager dùng để edit)
        Task<Service?> GetByIdAsync(int id);

        Task<List<Service>> GetAllAsync();
        // Cập nhật dịch vụ (Manager only)
        Task<bool> UpdateAsync(int id, decimal basePrice, string? description);

        Task<bool> UpdateAsync(int id, string serviceName, decimal basePrice, string? description, int? duration, bool isHomeService, bool status);
    }
}
