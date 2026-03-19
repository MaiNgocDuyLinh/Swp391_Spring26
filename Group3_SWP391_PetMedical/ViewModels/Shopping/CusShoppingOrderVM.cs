using System;
using System.Collections.Generic;
using System.Linq;

namespace Group3_SWP391_PetMedical.ViewModels.Shopping
{
    public class CusCheckoutVM
    {
        public List<CusCartItemVM> Items { get; set; } = new();
        public string? PaymentMethod { get; set; } = "Thanh toán tại quầy";
        public string? PickupNote { get; set; }
        public DateTime? PickupDate { get; set; }

        public List<int> SelectedCartItemIds { get; set; } = new();

        public decimal SubTotal => Items.Sum(x => x.LineTotal);
    }

    public class CusCheckoutSubmitVM
    {
        public string? PaymentMethod { get; set; }
        public string? PickupNote { get; set; }
        public DateTime? PickupDate { get; set; }

        public List<int> SelectedCartItemIds { get; set; } = new();
    }

    public class CusShoppingOrderListItemVM
    {
        public int OrderId { get; set; }
        public string OrderCode { get; set; } = "";
        public decimal TotalAmount { get; set; }
        public string PaymentStatus { get; set; } = "";
        public string OrderStatus { get; set; } = "";
        public DateTime CreatedAt { get; set; }
    }

    public class CusShoppingOrderDetailVM
    {
        public int OrderId { get; set; }
        public string OrderCode { get; set; } = "";
        public string? PaymentMethod { get; set; }
        public string PaymentStatus { get; set; } = "";
        public string OrderStatus { get; set; } = "";
        public string PickupMethod { get; set; } = "";
        public string? PickupNote { get; set; }
        public DateTime? PickupDate { get; set; }
        public DateTime CreatedAt { get; set; }

        public List<CusShoppingOrderDetailItemVM> Items { get; set; } = new();
        public decimal TotalAmount => Items.Sum(x => x.LineTotal);
    }

    public class CusShoppingOrderDetailItemVM
    {
        public string ProductName { get; set; } = "";
        public string? VariantName { get; set; }
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public decimal LineTotal { get; set; }
    }
}