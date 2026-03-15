using System;
using System.Collections.Generic;
using System.Linq;

namespace Group3_SWP391_PetMedical.ViewModels.Appointment
{
    public class CusMedicalRecordVM
    {
        public int AppointmentId { get; set; }
        public int MedicalRecordId { get; set; }

        public DateTime AppointmentDate { get; set; }
        public DateTime? CreatedAt { get; set; }

        public string PetName { get; set; } = "";
        public string? PetBreed { get; set; }
        public string? PetSpecies { get; set; }
        public string? PetGender { get; set; }
        public DateTime? PetBirthDate { get; set; }
        public double? PetWeight { get; set; }
        public string? PetImageUrl { get; set; }

        public string DoctorName { get; set; } = "";
        public string Status { get; set; } = "";

        public string? Diagnosis { get; set; }
        public string? HealthStatus { get; set; }
        public string? TestResults { get; set; }
        public string? ResultImages { get; set; }
        public DateOnly? FollowUpDate { get; set; }

        public string? SelectedServiceNames { get; set; }

        public string AppointmentShift
            => AppointmentDate.Hour < 12 ? "Ca sáng" : "Ca chiều";

        public List<CusMedicalRecordServiceItemVM> ExtraServices { get; set; } = new();
        public List<CusMedicalRecordPrescriptionItemVM> Prescriptions { get; set; } = new();

        public decimal TotalServiceAmount => ExtraServices.Sum(x => x.Price);
        public decimal TotalMedicineAmount => Prescriptions.Sum(x => x.LineTotal);
    }

    public class CusMedicalRecordServiceItemVM
    {
        public int ServiceId { get; set; }
        public string ServiceName { get; set; } = "";
        public decimal Price { get; set; }
        public string? Notes { get; set; }
    }

    public class CusMedicalRecordPrescriptionItemVM
    {
        public int MedicineId { get; set; }
        public string MedicineName { get; set; } = "";
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public string? Dosage { get; set; }

        public decimal LineTotal => UnitPrice * Quantity;
    }
}