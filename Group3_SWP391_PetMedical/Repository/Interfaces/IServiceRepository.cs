//using Group3_SWP391_PetMedical.Models;

//public interface IServiceRepository
//{
//    List<Service> GetAll(bool? onlyActive = true);
//    Service? GetById(int id);
//}
using Group3_SWP391_PetMedical.Models;
using Group3_SWP391_PetMedical.Models.Common;

public interface IServiceRepository
{
    Task<PagedResult<Service>> GetPagedAsync(string? q, int page, int pageSize);
}
