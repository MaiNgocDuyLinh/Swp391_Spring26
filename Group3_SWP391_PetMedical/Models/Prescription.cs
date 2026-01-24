using System;
using System.Collections.Generic;

namespace Group3_SWP391_PetMedical.Models;

public partial class Prescription
{
    public int prescription_id { get; set; }

    public int record_id { get; set; }

    public int medicine_id { get; set; }

    public string? dosage { get; set; }

    public int quantity { get; set; }

    public virtual Medication medicine { get; set; } = null!;

    public virtual MedicalRecord record { get; set; } = null!;
}
