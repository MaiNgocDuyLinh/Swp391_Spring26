using System.ComponentModel.DataAnnotations;

namespace Group3_SWP391_PetMedical.ViewModels.Shopping
{
    public class StaffShoppingUpdateOrderStatusVM
    {
        [Required]
        public int OrderId { get; set; }

        [Required]
        public string OrderStatus { get; set; } = string.Empty;

        [Required]
        public string PaymentStatus { get; set; } = string.Empty;
    }
}