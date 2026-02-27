using Group3_SWP391_PetMedical.Models;
using Group3_SWP391_PetMedical.Models.Common;

namespace Group3_SWP391_PetMedical.Services.Interfaces
{
    public interface IManagerService
    {
        // Services - Manager có thể xem và sửa toàn bộ
        Task<PagedResult<Service>> GetServicesPagedAsync(string? search, int page, int pageSize);
        Task<Service?> GetServiceByIdAsync(int id);
        Task<bool> UpdateServiceAsync(int id, string serviceName, decimal basePrice, string? description, int? duration, bool isHomeService, bool status);
    }
}
