using System;
using System.Collections.Generic;

namespace Group3_SWP391_PetMedical.Models;

public partial class Service
{
    public int service_id { get; set; }

    public string service_name { get; set; } = null!;

    public string? description { get; set; }

    public decimal base_price { get; set; }

    public int? duration { get; set; }

    public bool? is_home_service { get; set; }

    public bool? status { get; set; }

    public virtual ICollection<AppointmentDetail> AppointmentDetails { get; set; } = new List<AppointmentDetail>();
}
