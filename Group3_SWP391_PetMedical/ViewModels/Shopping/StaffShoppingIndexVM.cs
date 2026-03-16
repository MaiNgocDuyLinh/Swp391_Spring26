using Group3_SWP391_PetMedical.Models.Common;

namespace Group3_SWP391_PetMedical.ViewModels.Shopping
{
    public class StaffShoppingIndexVM
    {
        public StaffShoppingQuery Query { get; set; } = new();
        public List<CusShoppingCategoryVM> Categories { get; set; } = new();
        public PagedResult<StaffShoppingProductRowVM> Result { get; set; } = new();
    }
}