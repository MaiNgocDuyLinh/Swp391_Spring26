using System;
using System.Collections.Generic;

namespace Group3_SWP391_PetMedical.Models;

public partial class Schedule
{
    public int schedule_id { get; set; }

    public int doctor_id { get; set; }

    public DateOnly work_date { get; set; }

    public string? shift { get; set; }

    public string? status { get; set; }

    public virtual User doctor { get; set; } = null!;
}
