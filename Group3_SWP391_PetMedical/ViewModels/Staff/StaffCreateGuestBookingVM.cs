using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Group3_SWP391_PetMedical.ViewModels.Staff
{
    public class StaffCreateGuestBookingVM
    {
        // Customer Info
        [Required(ErrorMessage = "Vui lòng nhập họ tên khách hàng.")]
        public string CustomerName { get; set; } = "";

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại.")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ.")]
        public string CustomerPhone { get; set; } = "";

        public string? CustomerEmail { get; set; }

        // Pet Info
        [Required(ErrorMessage = "Vui lòng nhập tên thú cưng.")]
        public string PetName { get; set; } = "";

        [Required(ErrorMessage = "Vui lòng chọn loài.")]
        public string PetSpecies { get; set; } = "";

        public string? PetBreed { get; set; }
        public string? PetGender { get; set; }
        public double? PetWeight { get; set; }
        public DateTime? PetBirthdate { get; set; }

        // Appointment Info
        [Required(ErrorMessage = "Vui lòng chọn ngày khám.")]
        [DataType(DataType.Date)]
        public DateTime AppointmentDate { get; set; } = DateTime.Today;

        [Required(ErrorMessage = "Vui lòng chọn ca khám.")]
        public string Shift { get; set; } = "Sáng";

        [Required(ErrorMessage = "Vui lòng chọn bác sĩ.")]
        public int? DoctorId { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn ít nhất 1 dịch vụ.")]
        public List<int> ServiceIds { get; set; } = new List<int>();

        public string? Notes { get; set; }

        // Form Options
        public List<SelectListItem> PetSpeciesOptions { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> DoctorOptions { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> ServiceOptions { get; set; } = new List<SelectListItem>();
    }
}
