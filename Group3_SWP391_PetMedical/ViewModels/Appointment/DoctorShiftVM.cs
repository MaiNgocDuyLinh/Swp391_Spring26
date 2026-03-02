namespace Group3_SWP391_PetMedical.ViewModels.Appointment
{
    public class DoctorShiftVM
    {
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public string Display => $"{Start:HH:mm} - {End:HH:mm} ({Start:dd/MM/yyyy})";
    }
}