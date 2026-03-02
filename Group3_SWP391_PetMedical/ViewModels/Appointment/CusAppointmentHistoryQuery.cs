using Group3_SWP391_PetMedical.Models.Common;

namespace Group3_SWP391_PetMedical.ViewModels.Appointment
{
    public class CusAppointmentHistoryQuery : PagingQuery
    {
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int? ServiceId { get; set; }
    }
}