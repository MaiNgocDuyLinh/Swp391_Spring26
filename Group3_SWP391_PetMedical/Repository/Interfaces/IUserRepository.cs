using Group3_SWP391_PetMedical.Models;
using Group3_SWP391_PetMedical.Models.Common;

namespace Group3_SWP391_PetMedical.Repository.Interfaces
{
    public interface IUserRepository
    {
        Task<PagedResult<User>> GetCustomersPagedAsync(string? search, int page, int pageSize);
    }
}
