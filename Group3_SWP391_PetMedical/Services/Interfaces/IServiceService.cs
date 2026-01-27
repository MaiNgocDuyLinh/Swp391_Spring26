//using Group3_SWP391_PetMedical.Models;

//public interface IServiceService
//{
//    List<Service> GetServicesForDisplay();
//    Service? GetDetail(int id);
//}
using Group3_SWP391_PetMedical.Models;
using Group3_SWP391_PetMedical.Models.Common;

public interface IServiceService
{
    Task<PagedResult<Service>> GetServiceListAsync(PagingQuery query);
}
