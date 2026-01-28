using Group3_SWP391_PetMedical.Models;
using Group3_SWP391_PetMedical.Models.Common;

public interface IServiceRepository
{
    Task<PagedResult<Service>> GetPagedAsync(string? q, int page, int pageSize);
}
