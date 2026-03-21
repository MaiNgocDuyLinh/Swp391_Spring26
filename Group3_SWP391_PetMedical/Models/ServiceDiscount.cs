using System;

namespace Group3_SWP391_PetMedical.Models;

public partial class ServiceDiscount
{
    public int discount_id { get; set; }

    public int service_id { get; set; }

    public int discount_percent { get; set; }

    public DateTime start_date { get; set; }

    public DateTime end_date { get; set; }

    public bool? is_active { get; set; }

    public DateTime? created_at { get; set; }

    public virtual Service service { get; set; } = null!;
}
