using System;

namespace Group3_SWP391_PetMedical.Models;

/// <summary>
/// Bảng riêng: Yêu cầu đổi lịch làm việc của bác sĩ. Manager phê duyệt hoặc từ chối.
/// </summary>
public partial class ScheduleChangeRequest
{
    public int request_id { get; set; }

    public int doctor_id { get; set; }

    public int schedule_id { get; set; }

    public DateOnly requested_work_date { get; set; }

    public string requested_shift { get; set; } = null!;

    public string? reason { get; set; }

    /// <summary>Pending | Approved | Rejected</summary>
    public string status { get; set; } = "Pending";

    public DateTime? created_at { get; set; }

    public DateTime? decided_at { get; set; }

    public int? decided_by { get; set; }

    public string? manager_note { get; set; }

    public virtual User doctor { get; set; } = null!;
    public virtual Schedule schedule { get; set; } = null!;
    public virtual User? decidedByUser { get; set; }
}
