using Microsoft.AspNetCore.Mvc.Rendering;

namespace Group3_SWP391_PetMedical.ViewModels.Appointment
{
    public class CusCreateAppointmentVM
    {
        public CusCreateAppointmentCommand Form { get; set; } = new();

        public List<SelectListItem> PetOptions { get; set; } = new();
        public List<SelectListItem> ServiceOptions { get; set; } = new();
        public List<SelectListItem> DoctorOptions { get; set; } = new();
        public List<DoctorShiftVM> DoctorShifts { get; set; } = new(); // đổ vào view (optional)
    }
}