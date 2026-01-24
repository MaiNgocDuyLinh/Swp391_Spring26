using System;
using System.Collections.Generic;

namespace Group3_SWP391_PetMedical.Models;

public partial class Appointment
{
    public int appointment_id { get; set; }

    public int customer_id { get; set; }

    public int pet_id { get; set; }

    public int? doctor_id { get; set; }

    public DateTime appointment_date { get; set; }

    public string? status { get; set; }

    public string? notes { get; set; }

    public DateTime? created_at { get; set; }

    public virtual ICollection<AppointmentDetail> AppointmentDetails { get; set; } = new List<AppointmentDetail>();

    public virtual ICollection<Feedback> Feedbacks { get; set; } = new List<Feedback>();

    public virtual Invoice? Invoice { get; set; }

    public virtual MedicalRecord? MedicalRecord { get; set; }

    public virtual User customer { get; set; } = null!;

    public virtual User? doctor { get; set; }

    public virtual Pet pet { get; set; } = null!;
}
