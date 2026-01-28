using Group3_SWP391_PetMedical.Models.Common;

namespace Group3_SWP391_PetMedical.ViewModels.Pet
{
    public class MyPetsVm
    {
        public PagingQuery Query { get; set; } = new PagingQuery();
        public PagedResult<PetListItemVm> Result { get; set; } = new PagedResult<PetListItemVm>();

        // để dùng chung _Pager (PagedResult<object>)
        public PagedResult<object> Pager => new PagedResult<object>
        {
            Page = Result.Page,
            PageSize = Result.PageSize,
            TotalItems = Result.TotalItems,
            Items = Array.Empty<object>()
        };
    }
}
