using Group3_SWP391_PetMedical.ViewModels.Common;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Group3_SWP391_PetMedical.ViewModels.Appointment
{
    public class CusAppointmentHistoryListVM
    {
        public ListPageVM<CusAppointmentHistoryItemVM> Page { get; set; } = new();
        public CusAppointmentHistoryQuery Filter { get; set; } = new();
        public List<SelectListItem> ServiceOptions { get; set; } = new();
    }
}