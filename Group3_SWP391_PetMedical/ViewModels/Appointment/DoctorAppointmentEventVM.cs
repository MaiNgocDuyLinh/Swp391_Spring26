namespace Group3_SWP391_PetMedical.ViewModels.Appointment
{
    public class DoctorAppointmentEventVM
    {
        public string Title { get; set; } = "";
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public string? Status { get; set; }
    }
}