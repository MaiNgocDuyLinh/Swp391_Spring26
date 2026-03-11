namespace Group3_SWP391_PetMedical.ViewModels.Appointment
{
    public class DoctorShiftVM
    {
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public string Shift { get; set; } = "";
        public string Display => $"{Shift} ({Start:dd/MM/yyyy})";
    }
}