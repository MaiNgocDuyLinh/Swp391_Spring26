using System;
using System.Collections.Generic;

namespace Group3_SWP391_PetMedical.Models;

public partial class Feedback
{
    public int feedback_id { get; set; }

    public int appointment_id { get; set; }

    public int customer_id { get; set; }

    public int? rating { get; set; }

    public string? comment { get; set; }

    public DateTime? created_at { get; set; }

    public virtual Appointment appointment { get; set; } = null!;

    public virtual User customer { get; set; } = null!;
}
