namespace Group3_SWP391_PetMedical.ViewModels.Feedback
{
    public class CusFeedbackHistoryItemVM
    {
        public int AppointmentId { get; set; }
        public DateTime AppointmentDate { get; set; }

        public string PetName { get; set; } = "";
        public string DoctorName { get; set; } = "";
        public string ServiceNames { get; set; } = "";
        public string Status { get; set; } = "";

        public decimal? TotalAmount { get; set; }
        public string? Notes { get; set; }

        public bool HasFeedback { get; set; }
        public int? Rating { get; set; }
        public string? Comment { get; set; }
        public DateTime? FeedbackCreatedAt { get; set; }
    }
}