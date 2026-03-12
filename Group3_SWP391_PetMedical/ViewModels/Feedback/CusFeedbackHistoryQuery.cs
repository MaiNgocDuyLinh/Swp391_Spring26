using Group3_SWP391_PetMedical.Models.Common;

namespace Group3_SWP391_PetMedical.ViewModels.Feedback
{
    public class CusFeedbackHistoryQuery : PagingQuery
    {
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int? ServiceId { get; set; }
    }
}