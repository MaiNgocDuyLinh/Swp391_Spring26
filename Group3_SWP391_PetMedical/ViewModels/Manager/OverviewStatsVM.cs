namespace Group3_SWP391_PetMedical.ViewModels.Manager;

/// <summary>
/// View model cho trang Thống kê tổng quan (Manager).
/// </summary>
public class OverviewStatsVM
{
    /// <summary> Doanh thu (tổng từ hóa đơn đã thanh toán) trong kỳ. </summary>
    public decimal Revenue { get; set; }

    /// <summary> Số lượt đăng nhập của khách hàng (Customer) trong kỳ. </summary>
    public int CustomerLoginCount { get; set; }

    /// <summary> Tổng số lịch khám đã đặt trong kỳ. </summary>
    public int TotalAppointments { get; set; }

    /// <summary> Số lịch khám theo từng status (status -> count). </summary>
    public Dictionary<string, int> AppointmentsByStatus { get; set; } = new();

    /// <summary> Doanh thu theo ngày (label -> value) cho biểu đồ. </summary>
    public List<RevenueByDateItem> RevenueByDate { get; set; } = new();

    /// <summary> Số lịch khám theo ngày cho biểu đồ. </summary>
    public List<AppointmentsByDateItem> AppointmentsByDate { get; set; } = new();

    public string? GroupBy { get; set; }  // "day" | "month"
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
}

public class RevenueByDateItem
{
    public string Label { get; set; } = "";  // "dd/MM" hoặc "MM/yyyy"
    public decimal Value { get; set; }
}

public class AppointmentsByDateItem
{
    public string Label { get; set; } = "";
    public int Count { get; set; }
}
