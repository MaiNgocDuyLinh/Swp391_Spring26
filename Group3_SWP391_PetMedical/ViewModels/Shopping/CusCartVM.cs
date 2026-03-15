using System.Collections.Generic;
using System.Linq;

namespace Group3_SWP391_PetMedical.ViewModels.Shopping
{
    public class CusCartVM
    {
        public List<CusCartItemVM> Items { get; set; } = new();
        public decimal SubTotal => Items.Sum(x => x.LineTotal);
    }

    public class CusCartItemVM
    {
        public int CartItemId { get; set; }
        public int ProductId { get; set; }
        public int? VariantId { get; set; }
        public string ProductName { get; set; } = "";
        public string? VariantName { get; set; }
        public string? ImageUrl { get; set; }
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public decimal LineTotal => UnitPrice * Quantity;
    }
}