namespace Group3_SWP391_PetMedical.ViewModels.Order
{
    public class OrderHistoryVm
    {
        public int OrderId { get; set; }
        public DateTime? CreatedAt { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; }
        public string? PickupSlot { get; set; }
        public string? Note { get; set; }

        // Danh sách tên các loại thuốc trong đơn này để hiển thị nhanh
        public List<string> MedicineNames { get; set; } = new List<string>();
    }
}
