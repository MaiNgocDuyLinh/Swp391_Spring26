using System;
using System.Collections.Generic;

namespace Group3_SWP391_PetMedical.Models;

public partial class MedicalRecord
{
    public int record_id { get; set; }

    public int appointment_id { get; set; }

    public string? diagnosis { get; set; }

    public string? health_status { get; set; }

    public string? test_results { get; set; }

    public string? result_images { get; set; }

    public DateOnly? follow_up_date { get; set; }

    public DateTime? created_at { get; set; }

    public virtual ICollection<Prescription> Prescriptions { get; set; } = new List<Prescription>();

    public virtual Appointment appointment { get; set; } = null!;
}
