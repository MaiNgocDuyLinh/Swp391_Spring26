using System.Collections.Generic;

namespace Group3_SWP391_PetMedical.ViewModels.Shopping
{
    public class CusShoppingDetailVM
    {
        public int ProductId { get; set; }
        public int CategoryId { get; set; }
        public string Name { get; set; } = "";
        public string CategoryName { get; set; } = "";
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
        public string Status { get; set; } = "";
        public string? ImageUrl { get; set; }

        public int Quantity { get; set; } = 1;
        public int? SelectedVariantId { get; set; }

        public List<CusShoppingVariantVM> Variants { get; set; } = new();


    }

    public class CusShoppingVariantVM
    {
        public int VariantId { get; set; }
        public string VariantName { get; set; } = "";
        public string? Color { get; set; }
        public string? Size { get; set; }
        public string? Material { get; set; }
        public decimal? PriceOverride { get; set; }
        public int StockQuantity { get; set; }
        public string Status { get; set; } = "";

        public int? SelectedVariantId { get; set; }
    }

    public class CusAddToCartVM
    {
        public int ProductId { get; set; }
        public int? VariantId { get; set; }
        public int Quantity { get; set; } = 1;
    }
}