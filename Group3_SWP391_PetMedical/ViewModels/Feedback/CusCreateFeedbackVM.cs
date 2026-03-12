using System.ComponentModel.DataAnnotations;

namespace Group3_SWP391_PetMedical.ViewModels.Feedback
{
    public class CusCreateFeedbackVM
    {
        public int AppointmentId { get; set; }

        public DateTime AppointmentDate { get; set; }

        public string PetName { get; set; } = "";
        public string DoctorName { get; set; } = "";
        public string ServiceNames { get; set; } = "";

        [Required(ErrorMessage = "Vui lòng chọn số sao.")]
        [Range(1, 5, ErrorMessage = "Số sao phải từ 1 đến 5.")]
        public int? Rating { get; set; }

        [StringLength(1000, ErrorMessage = "Nội dung phản hồi tối đa 1000 ký tự.")]
        public string? Comment { get; set; }
    }
}