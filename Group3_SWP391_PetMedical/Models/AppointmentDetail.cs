using System;
using System.Collections.Generic;

namespace Group3_SWP391_PetMedical.Models;

public partial class AppointmentDetail
{
    public int appointment_id { get; set; }

    public int service_id { get; set; }

    public decimal? actual_price { get; set; }

    public virtual Appointment appointment { get; set; } = null!;

    public virtual Service service { get; set; } = null!;
}
