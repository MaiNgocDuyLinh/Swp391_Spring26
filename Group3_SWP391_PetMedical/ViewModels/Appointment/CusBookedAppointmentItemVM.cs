using System;

namespace Group3_SWP391_PetMedical.ViewModels.Appointment
{
    public class CusBookedAppointmentItemVM
    {
        public int AppointmentId { get; set; }
        public DateTime AppointmentDate { get; set; }
        public string PetName { get; set; } = "";
        public string DoctorName { get; set; } = "";
        public string Status { get; set; } = "";
        public string? Notes { get; set; }
        public string ServiceNames { get; set; } = "";
        public decimal? TotalAmount { get; set; }  // nếu có invoice mà chưa trả / hoặc hiển thị tạm
    }
}