namespace Group3_SWP391_PetMedical.ViewModels.Shopping
{
    public class StaffShoppingOrderRowVM
    {
        public int OrderId { get; set; }
        public string OrderCode { get; set; } = string.Empty;
        public int CustomerId { get; set; }
        public string? CustomerName { get; set; }
        public decimal TotalAmount { get; set; }
        public string PaymentStatus { get; set; } = string.Empty;
        public string OrderStatus { get; set; } = string.Empty;
        public string PickupMethod { get; set; } = string.Empty;
        public DateTime? PickupDate { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}