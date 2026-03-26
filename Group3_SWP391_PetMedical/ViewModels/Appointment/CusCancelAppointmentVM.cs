using System.ComponentModel.DataAnnotations;

namespace Group3_SWP391_PetMedical.ViewModels.Appointment
{
    public class CusCancelAppointmentVM
    {
        public int AppointmentId { get; set; }

        // ===== Thông tin tóm tắt hiển thị =====
        public DateTime AppointmentDate { get; set; }
        public string PetName { get; set; } = "";
        public string ServiceNames { get; set; } = "";

        // ===== Dữ liệu hủy =====
        public string? Description { get; set; }     // ghi chú cũ nếu cần
        [Required(ErrorMessage = "Vui lòng nhập lý do hủy.")]
        public string Reason { get; set; } = "";

    }
}