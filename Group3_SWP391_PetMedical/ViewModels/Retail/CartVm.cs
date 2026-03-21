using System.Collections.Generic;
using System.Linq;

namespace Group3_SWP391_PetMedical.ViewModels.Retail;

public class CartVm
{
    public int cart_id { get; set; }
    public int user_id { get; set; }
    public string status { get; set; } = null!;

    public List<CartItemVm> items { get; set; } = new();

    public int total_quantity => items.Sum(i => i.quantity);
    public decimal total_amount => items.Sum(i => i.line_total);
}

