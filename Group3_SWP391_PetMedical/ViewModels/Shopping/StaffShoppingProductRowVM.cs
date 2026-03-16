namespace Group3_SWP391_PetMedical.ViewModels.Shopping
{
    public class StaffShoppingProductRowVM
    {
        public int ProductId { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;
        public string SKU { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }

        public int VariantCount { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}