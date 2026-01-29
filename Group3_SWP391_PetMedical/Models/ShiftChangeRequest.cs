using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Group3_SWP391_PetMedical.Models
{
    [Table("ShiftChangeRequests")] // Tên bảng trong Database
    public class ShiftChangeRequest
    {
        [Key]
        public int request_id { get; set; }

        public int doctor_id { get; set; } // Người gửi yêu cầu

        public DateTime request_date { get; set; } // Ngày muốn nghỉ/đổi

        public string? reason { get; set; } // Lý do

        public string? status { get; set; } // 'Pending', 'Approved', 'Rejected'

        public DateTime? created_at { get; set; }

        // Kết nối với bảng User (Doctor)
        [ForeignKey("doctor_id")]
        public virtual User? Doctor { get; set; }
    }
}