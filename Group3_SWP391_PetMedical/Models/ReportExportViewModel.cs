using System;
using System.Collections.Generic;

namespace Group3_SWP391_PetMedical.Models;

public class ReportExportViewModel
{
    public DateTime? FromDate { get; set; }

    public DateTime? ToDate { get; set; }

    /// <summary>
    /// Invoices | Prescriptions | Statistics
    /// </summary>
    public string ExportType { get; set; } = "Invoices";

    public List<Invoice> Invoices { get; set; } = new();

    public List<Prescription> Prescriptions { get; set; } = new();

    public List<ReportStatisticsRow> Statistics { get; set; } = new();
}

public class ReportStatisticsRow
{
    public DateTime Date { get; set; }

    public decimal TotalRevenue { get; set; }

    public int AppointmentCount { get; set; }
}

