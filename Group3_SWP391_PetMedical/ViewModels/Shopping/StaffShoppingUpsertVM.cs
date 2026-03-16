using System.ComponentModel.DataAnnotations;

namespace Group3_SWP391_PetMedical.ViewModels.Shopping
{
    public class StaffShoppingUpsertVM
    {
        public int? ProductId { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn danh mục.")]
        public int CategoryId { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên sản phẩm.")]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập SKU.")]
        [StringLength(50)]
        public string SKU { get; set; } = string.Empty;

        [Range(0, double.MaxValue, ErrorMessage = "Giá phải >= 0")]
        public decimal Price { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Tồn kho phải >= 0")]
        public int StockQuantity { get; set; }

        [Required]
        public string Status { get; set; } = "Đang bán";

        public string? Description { get; set; }
        public string? ImageUrl { get; set; }

        public List<CusShoppingCategoryVM> Categories { get; set; } = new();
        public List<StaffShoppingVariantInputVM> Variants { get; set; } = new();
    }
}