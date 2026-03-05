namespace Group3_SWP391_PetMedical.ViewModels.Manager;

/// <summary>
/// Dòng trong danh sách yêu cầu đổi lịch (Manager xem).
/// </summary>
public class ScheduleChangeRequestListVM
{
    public int RequestId { get; set; }
    public int DoctorId { get; set; }
    public string DoctorName { get; set; } = "";
    public int ScheduleId { get; set; }
    public DateOnly CurrentWorkDate { get; set; }
    public string? CurrentShift { get; set; }
    public DateOnly RequestedWorkDate { get; set; }
    public string RequestedShift { get; set; } = "";
    public string? Reason { get; set; }
    public string Status { get; set; } = "";
    public DateTime? CreatedAt { get; set; }
}
