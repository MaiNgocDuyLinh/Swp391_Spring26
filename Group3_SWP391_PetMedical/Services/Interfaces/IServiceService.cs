using Group3_SWP391_PetMedical.Models;
using Group3_SWP391_PetMedical.Models.Common;

public interface IServiceService
{
    Task<PagedResult<Service>> GetServiceListAsync(PagingQuery query);
}
