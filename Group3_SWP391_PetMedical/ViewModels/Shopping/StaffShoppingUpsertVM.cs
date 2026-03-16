using Microsoft.AspNetCore.Http;

namespace Group3_SWP391_PetMedical.ViewModels.Shopping
{
    public class StaffShoppingUpsertVM
    {
        public int? ProductId { get; set; }

        public int CategoryId { get; set; }

        public string? Name { get; set; }

        public string? SKU { get; set; }

        public decimal Price { get; set; }

        public int StockQuantity { get; set; }

        public string? Status { get; set; }

        public string? Description { get; set; }

        public string? ImageUrl { get; set; }

        public IFormFile? ImageFile { get; set; }

        public List<CusShoppingCategoryVM> Categories { get; set; } = new();

        public List<StaffShoppingVariantInputVM> Variants { get; set; } = new();
    }
}