using System.Collections.Generic;

namespace Group3_SWP391_PetMedical.ViewModels.Retail
{
    public class CheckoutItemVm
    {
        public int medicine_id { get; set; }
        public string medicine_name { get; set; } = null!;
        public decimal unit_price { get; set; }
        public int quantity { get; set; }
        public int stock_quantity { get; set; }

        public decimal line_total => unit_price * quantity;
    }

    public class CheckoutVm
    {
        public List<CheckoutItemVm> Items { get; set; } = new();
        public decimal TotalAmount { get; set; }

        public string? PickupSlot { get; set; }
        public DateTime? PickupDate { get; set; }
        public string? Note { get; set; }
        public string PaymentMethod { get; set; } = "ONLINE";

        public List<string> StockErrors { get; set; } = new();

        public int[] SelectedMedicineIds { get; set; } = new int[0];
    }

    public class CheckoutOrderVm
    {
        public int OrderId { get; set; }
        public decimal TotalAmount { get; set; }
        public string? PickupSlot { get; set; }
        public DateTime? PickupDate { get; set; }
        public string? Note { get; set; }

        public List<CheckoutItemVm> Items { get; set; } = new();
        public string QrImageUrl { get; set; } = null!;
    }
}
