using System;
using System.Collections.Generic;

namespace Group3_SWP391_PetMedical.Models;

public partial class Role
{
    public int role_id { get; set; }

    public string role_name { get; set; } = null!;

    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
