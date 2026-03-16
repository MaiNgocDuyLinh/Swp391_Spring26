using System.ComponentModel.DataAnnotations;

namespace Group3_SWP391_PetMedical.ViewModels.Shopping
{
    public class StaffShoppingVariantInputVM
    {
        public int? VariantId { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên biến thể.")]
        [StringLength(255)]
        public string VariantName { get; set; } = string.Empty;

        [StringLength(50)]
        public string? Color { get; set; }

        [StringLength(50)]
        public string? Size { get; set; }

        [StringLength(100)]
        public string? Material { get; set; }

        [StringLength(50)]
        public string? SKU { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Giá phải >= 0")]
        public decimal? PriceOverride { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Tồn kho phải >= 0")]
        public int StockQuantity { get; set; }

        [Required]
        public string Status { get; set; } = "Đang bán";

        [StringLength(255)]
        public string? ImageUrl { get; set; }
    }
}