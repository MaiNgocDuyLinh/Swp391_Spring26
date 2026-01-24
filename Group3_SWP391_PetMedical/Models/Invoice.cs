using System;
using System.Collections.Generic;

namespace Group3_SWP391_PetMedical.Models;

public partial class Invoice
{
    public int invoice_id { get; set; }

    public int appointment_id { get; set; }

    public decimal total_amount { get; set; }

    public string? payment_status { get; set; }

    public string? payment_method { get; set; }

    public string? vnpay_transaction_id { get; set; }

    public DateTime? created_at { get; set; }

    public virtual Appointment appointment { get; set; } = null!;
}
