using System;
using System.Collections.Generic;

namespace Group3_SWP391_PetMedical.Models;

public partial class CartMedicin
{
    public int id { get; set; }

    public int user_id { get; set; }

    public string status { get; set; } = null!;

    public DateTime? created_at { get; set; }

    public virtual User user { get; set; } = null!;

    public virtual ICollection<CartItemMedicin> CartItemsMedicin { get; set; } = new List<CartItemMedicin>();
}

