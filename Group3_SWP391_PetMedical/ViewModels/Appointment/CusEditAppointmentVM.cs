using Microsoft.AspNetCore.Mvc.Rendering;

namespace Group3_SWP391_PetMedical.ViewModels.Appointment
{
    public class CusEditAppointmentVM
    {
        public int AppointmentId { get; set; }

        public DateTime AppointmentDate { get; set; }  // cho phép đổi ngày/giờ
        public string? Notes { get; set; }             // cho phép đổi mô tả/ghi chú

        // nếu bạn muốn cho đổi dịch vụ: dùng list checkbox (serviceIds)
        public List<int> ServiceIds { get; set; } = new();

        // để render checkbox
        public List<SelectListItem> AllServices { get; set; } = new();

        // readonly info
        public DateTime CreatedAt { get; set; }
        public string Status { get; set; } = "";       // hiển thị nhưng KHÔNG cho sửa
    }
}