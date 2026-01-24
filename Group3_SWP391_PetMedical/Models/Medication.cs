using System;
using System.Collections.Generic;

namespace Group3_SWP391_PetMedical.Models;

public partial class Medication
{
    public int medicine_id { get; set; }

    public string name { get; set; } = null!;

    public decimal unit_price { get; set; }

    public int? stock_quantity { get; set; }

    public string? description { get; set; }

    public virtual ICollection<Prescription> Prescriptions { get; set; } = new List<Prescription>();
}
