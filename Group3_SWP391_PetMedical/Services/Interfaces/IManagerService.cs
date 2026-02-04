using Group3_SWP391_PetMedical.Models;
using Group3_SWP391_PetMedical.Models.Common;

namespace Group3_SWP391_PetMedical.Services.Interfaces
{
    public interface IManagerService
    {
        // Services - Manager có thể xem và sửa
        Task<PagedResult<Service>> GetServicesPagedAsync(string? search, int page, int pageSize);
        Task<Service?> GetServiceByIdAsync(int id);
        Task<bool> UpdateServiceAsync(int id, decimal basePrice, string? description);
    }
}
