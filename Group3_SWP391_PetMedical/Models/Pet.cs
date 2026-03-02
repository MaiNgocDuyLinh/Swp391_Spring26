


using System;
using System.Collections.Generic;

namespace Group3_SWP391_PetMedical.Models;

public partial class Pet
{
    public int pet_id { get; set; }

    public int owner_id { get; set; }

    public string name { get; set; } = null!;

    public string? breed { get; set; }

    public string? species { get; set; }

    //   Giới tính
    public string? pet_gender { get; set; }

    //   Ngày sinh  
    public DateTime? pet_birthdate { get; set; }

    public int? age { get; set; }

    public double? weight { get; set; }

    public DateTime? created_at { get; set; }

    public string? PetImg { get; set; }

    public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();

    public virtual User owner { get; set; } = null!;
}
