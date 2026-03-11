using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace Group3_SWP391_PetMedical.ViewModels.Appointment
{
    public class CusEditAppointmentVM
    {
        public int AppointmentId { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn ngày khám.")]
        public DateTime AppointmentDate { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn ca khám.")]
        public string Shift { get; set; } = "";

        public string? Notes { get; set; }

        public List<int> ServiceIds { get; set; } = new();

        public List<SelectListItem> AllServices { get; set; } = new();

        public DateTime CreatedAt { get; set; }
        public string Status { get; set; } = "";
    }
}