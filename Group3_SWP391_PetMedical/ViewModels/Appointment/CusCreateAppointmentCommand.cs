using System.ComponentModel.DataAnnotations;

namespace Group3_SWP391_PetMedical.ViewModels.Appointment
{
    public class CusCreateAppointmentCommand
    {
        [Required(ErrorMessage = "Vui lòng chọn thú cưng.")]
        public int PetId { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn ngày khám.")]
        public DateTime AppointmentDate { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn ca khám.")]
        public string Shift { get; set; } = "";

        [MinLength(1, ErrorMessage = "Vui lòng chọn ít nhất 1 dịch vụ.")]
        public List<int> ServiceIds { get; set; } = new();

        public int? DoctorId { get; set; }  
        public string? Notes { get; set; }
        public bool IgnoreDoctorShiftCheck { get; set; } = false;
    }
}