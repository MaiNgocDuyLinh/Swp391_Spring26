using Group3_SWP391_PetMedical.Models;
using Group3_SWP391_PetMedical.Models.Common;

public class ServiceService : IServiceService
{
    private readonly IServiceRepository _repo;
    public ServiceService(IServiceRepository repo) => _repo = repo;

    public Task<PagedResult<Service>> GetServiceListAsync(PagingQuery query)
        => _repo.GetPagedAsync(query.Q, query.Page, query.PageSize);
}
