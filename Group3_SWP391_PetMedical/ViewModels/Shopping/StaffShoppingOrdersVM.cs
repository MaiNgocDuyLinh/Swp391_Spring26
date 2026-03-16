using Group3_SWP391_PetMedical.Models.Common;

namespace Group3_SWP391_PetMedical.ViewModels.Shopping
{
    public class StaffShoppingOrdersVM
    {
        public StaffShoppingOrderQuery Query { get; set; } = new();
        public PagedResult<StaffShoppingOrderRowVM> Result { get; set; } = new();
    }
}