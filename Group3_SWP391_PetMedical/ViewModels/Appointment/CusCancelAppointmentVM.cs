namespace Group3_SWP391_PetMedical.ViewModels.Appointment
{
    public class CusCancelAppointmentVM
    {
        public int AppointmentId { get; set; }
        public string? Description { get; set; }     // notes/description hiện tại
        public string Reason { get; set; } = "";     // lý do hủy
    }
}