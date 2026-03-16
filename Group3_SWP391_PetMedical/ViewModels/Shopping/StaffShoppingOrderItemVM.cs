namespace Group3_SWP391_PetMedical.ViewModels.Shopping
{
    public class StaffShoppingOrderItemVM
    {
        public string ProductName { get; set; } = string.Empty;
        public string? VariantName { get; set; }
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public decimal LineTotal { get; set; }
    }
}