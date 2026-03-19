namespace Group3_SWP391_PetMedical.ViewModels.Shopping
{
    public class StaffShoppingOrderDetailVM
    {
        public int OrderId { get; set; }
        public int CustomerId { get; set; }
        public string OrderCode { get; set; } = string.Empty;
        public string? CustomerName { get; set; }
        public decimal TotalAmount { get; set; }

        public string? PaymentMethod { get; set; }
        public string PaymentStatus { get; set; } = string.Empty;
        public string OrderStatus { get; set; } = string.Empty;
        public string PickupMethod { get; set; } = string.Empty;
        public string? PickupNote { get; set; }
        public DateTime? PickupDate { get; set; }
        public DateTime CreatedAt { get; set; }

        public bool IsCancelled { get; set; }
        public bool IsStatusUpdateLocked { get; set; }
        public string? StatusUpdateLockMessage { get; set; }

        public List<StaffShoppingOrderItemVM> Items { get; set; } = new();
    }
}