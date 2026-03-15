using Group3_SWP391_PetMedical.Models.Common;

namespace Group3_SWP391_PetMedical.ViewModels.Shopping
{
    public class CusShoppingMyOrdersVM
    {
        public CusShoppingOrderQuery Filter { get; set; } = new();
        public PagedResult<CusShoppingOrderListItemVM> Page { get; set; } = new();
        public decimal FilteredTotalAmount { get; set; }
    }
}