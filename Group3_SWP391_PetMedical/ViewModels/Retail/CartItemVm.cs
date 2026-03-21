namespace Group3_SWP391_PetMedical.ViewModels.Retail;

public class CartItemVm
{
    public int medicine_id { get; set; }
    public string medicine_name { get; set; } = null!;
    public decimal unit_price { get; set; }
    public int quantity { get; set; }
    public int stock_quantity { get; set; }

    public decimal line_total => unit_price * quantity;
}

