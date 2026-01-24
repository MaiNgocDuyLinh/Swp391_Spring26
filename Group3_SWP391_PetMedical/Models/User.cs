using System;
using System.Collections.Generic;

namespace Group3_SWP391_PetMedical.Models;

public partial class User
{
    public int user_id { get; set; }

    public int role_id { get; set; }

    public string email { get; set; } = null!;

    public string password { get; set; } = null!;

    public string full_name { get; set; } = null!;

    public string? phone { get; set; }

    public string? avatar { get; set; }

    public string? status { get; set; }

    public string? verification_token { get; set; }

    public DateTime? created_at { get; set; }

    public virtual ICollection<Appointment> Appointmentcustomers { get; set; } = new List<Appointment>();

    public virtual ICollection<Appointment> Appointmentdoctors { get; set; } = new List<Appointment>();

    public virtual ICollection<Feedback> Feedbacks { get; set; } = new List<Feedback>();

    public virtual ICollection<Pet> Pets { get; set; } = new List<Pet>();

    public virtual ICollection<Schedule> Schedules { get; set; } = new List<Schedule>();

    public virtual Role role { get; set; } = null!;
}
