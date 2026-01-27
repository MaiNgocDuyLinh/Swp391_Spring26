//using Group3_SWP391_PetMedical.Models;

//public class ServiceService : IServiceService
//{
//    private readonly IServiceRepository _repo;

//    public ServiceService(IServiceRepository repo)
//    {
//        _repo = repo;
//    }

//    public List<Service> GetServicesForDisplay()
//    {
//        // có thể thêm logic: sắp xếp, lọc theo status, v.v.
//        return _repo.GetAll(onlyActive: true);
//    }

//    public Service? GetDetail(int id)
//    {
//        return _repo.GetById(id);
//    }
//}
using Group3_SWP391_PetMedical.Models;
using Group3_SWP391_PetMedical.Models.Common;

public class ServiceService : IServiceService
{
    private readonly IServiceRepository _repo;
    public ServiceService(IServiceRepository repo) => _repo = repo;

    public Task<PagedResult<Service>> GetServiceListAsync(PagingQuery query)
        => _repo.GetPagedAsync(query.Q, query.Page, query.PageSize);
}
