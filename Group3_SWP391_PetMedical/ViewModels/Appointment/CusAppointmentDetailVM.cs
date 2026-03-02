namespace Group3_SWP391_PetMedical.ViewModels.Appointment
{
    public class CusAppointmentDetailVM
    {
        public int AppointmentId { get; set; }
        public DateTime AppointmentDate { get; set; }

        public DateTime CreatedAt { get; set; }     // ⚠️ map theo field thật của Appointment
        public string Status { get; set; } = "";
        public string? Notes { get; set; }          // coi như description

        public string PetName { get; set; } = "";
        public string CustomerName { get; set; } = "";   // nếu có
        public string DoctorName { get; set; } = "Chưa phân công";

        public List<string> Services { get; set; } = new();
        public decimal? TotalAmount { get; set; }

        public bool CanEdit { get; set; }           // controller set
        public bool CanCancel { get; set; }         // controller set
    }
}