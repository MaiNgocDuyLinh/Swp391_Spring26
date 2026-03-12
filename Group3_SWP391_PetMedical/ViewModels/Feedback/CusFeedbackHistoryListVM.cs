using Group3_SWP391_PetMedical.ViewModels.Common;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Group3_SWP391_PetMedical.ViewModels.Feedback
{
    public class CusFeedbackHistoryListVM
    {
        public ListPageVM<CusFeedbackHistoryItemVM> Page { get; set; } = new();
        public CusFeedbackHistoryQuery Filter { get; set; } = new();
        public List<SelectListItem> ServiceOptions { get; set; } = new();
    }
}