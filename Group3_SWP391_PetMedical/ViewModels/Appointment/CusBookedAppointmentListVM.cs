using System.Collections.Generic;
using Group3_SWP391_PetMedical.ViewModels.Common;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Group3_SWP391_PetMedical.ViewModels.Appointment
{
    public class CusBookedAppointmentListVM
    {
        public CusBookedAppointmentQuery Filter { get; set; } = new();
        public ListPageVM<CusBookedAppointmentItemVM> Page { get; set; } = new();
        public List<SelectListItem> ServiceOptions { get; set; } = new();
    }
}