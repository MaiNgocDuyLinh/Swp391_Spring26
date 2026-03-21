using System;
using System.Collections.Generic;

namespace Group3_SWP391_PetMedical.Models;

public partial class RetailOrder
{
    public int id { get; set; }

    public int? user_id { get; set; }

    public decimal total_amount { get; set; }

    public string status { get; set; } = null!;

    public DateTime? created_at { get; set; }

    public string? transaction_reference { get; set; }

    public string? pickup_slot { get; set; }

    public string? note { get; set; }

    public virtual User? user { get; set; }

    public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
}

