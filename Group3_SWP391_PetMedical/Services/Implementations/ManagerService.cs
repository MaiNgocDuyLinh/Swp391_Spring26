using Group3_SWP391_PetMedical.Models;
using Group3_SWP391_PetMedical.Models.Common;
using Group3_SWP391_PetMedical.Repository.Interfaces;
using Group3_SWP391_PetMedical.Services.Interfaces;

namespace Group3_SWP391_PetMedical.Services.Implementations
{
    public class ManagerService : IManagerService
    {
        private readonly IServiceRepository _serviceRepo;

        public ManagerService(IServiceRepository serviceRepo)
        {
            _serviceRepo = serviceRepo;
        }

        // ========== SERVICES ==========
        public Task<PagedResult<Service>> GetServicesPagedAsync(string? search, int page, int pageSize)
        {
            return _serviceRepo.GetPagedAsync(search, page, pageSize);
        }

        public Task<Service?> GetServiceByIdAsync(int id)
        {
            return _serviceRepo.GetByIdAsync(id);
        }

        public Task<bool> UpdateServiceAsync(int id, decimal basePrice, string? description)
        {
            return _serviceRepo.UpdateAsync(id, basePrice, description);
        }
    }
}
