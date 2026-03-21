using System;
using System.Collections.Generic;

namespace Group3_SWP391_PetMedical.Models;

public partial class OrderDetail
{
    public int order_id { get; set; }

    public int medicine_id { get; set; }

    public int quantity { get; set; }

    public virtual RetailOrder order { get; set; } = null!;

    public virtual Medication medicine { get; set; } = null!;
}

