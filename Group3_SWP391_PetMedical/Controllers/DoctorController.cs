using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Group3_SWP391_PetMedical.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using System.IO;
using System.Linq;

namespace Group3_SWP391_PetMedical.Controllers
{
    [Authorize(Roles = "Doctor")]
    public class DoctorController : Controller
    {
        private readonly PetClinicContext _context;

        public DoctorController(PetClinicContext context)
        {
            _context = context;
        }

        // 1. Trang danh sách lịch khám
        public async Task<IActionResult> Index(DateTime? date)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdClaim, out int currentDoctorId))
            {
                return RedirectToAction("Index", "Login");
            }

            DateTime selectedDate = date ?? DateTime.Today;
            ViewBag.SelectedDate = selectedDate.ToString("yyyy-MM-dd");

            var appointments = await _context.Appointments
                .Include(a => a.pet)
                .Include(a => a.customer)
                .Where(a => a.doctor_id == currentDoctorId &&
                            a.appointment_date.Date == selectedDate.Date)
                .OrderBy(a => a.appointment_date)
                .ToListAsync();

            return View(appointments);
        }

        // 2. Trang thực hiện khám bệnh
        public async Task<IActionResult> PerformExamination(int id)
        {
            var appointment = await _context.Appointments
                .Include(a => a.pet)
                .Include(a => a.customer)
                .FirstOrDefaultAsync(a => a.appointment_id == id);

            if (appointment == null) return NotFound();

            if (appointment.status == "Đã đến")
            {
                appointment.status = "Đang khám";
                _context.Update(appointment);
                await _context.SaveChangesAsync();
            }

            ViewBag.Medicines = await _context.Medications.ToListAsync();
            ViewBag.Services = await _context.Services.ToListAsync();

            ViewBag.History = await _context.MedicalRecords
                .Include(m => m.appointment)
                .Where(m => m.appointment.pet_id == appointment.pet_id)
                .OrderByDescending(m => m.created_at)
                .ToListAsync();

            return View(appointment);
        }

        // 3. Xử lý lưu kết quả khám
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveExamination(
            int appointment_id,
            string Diagnosis,
            string DoctorNotes,
            List<ServiceInput> SelectedServices,
            List<MedicineInput> Prescriptions,
            List<IFormFile> ExamImages)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var appointment = await _context.Appointments
                    .Include(a => a.pet)
                    .FirstOrDefaultAsync(a => a.appointment_id == appointment_id);

                if (appointment == null) return NotFound();

                var record = new MedicalRecord
                {
                    appointment_id = appointment_id,
                    diagnosis = Diagnosis,
                    test_results = DoctorNotes,
                    created_at = DateTime.Now,
                    health_status = "Đã Khám"
                };

                if (ExamImages != null && ExamImages.Any())
                {
                    var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads/exams");
                    if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

                    List<string> imageNames = new List<string>();
                    foreach (var file in ExamImages)
                    {
                        var fileName = $"{Guid.NewGuid()}_{file.FileName}";
                        var filePath = Path.Combine(folderPath, fileName);
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await file.CopyToAsync(stream);
                        }
                        imageNames.Add($"/uploads/exams/{fileName}");
                    }
                    record.result_images = string.Join(";", imageNames);
                }

                _context.MedicalRecords.Add(record);
                await _context.SaveChangesAsync();

                if (SelectedServices != null)
                {
                    foreach (var item in SelectedServices.Where(s => s.service_id > 0))
                    {
                        var exists = await _context.AppointmentDetails
                            .AnyAsync(ad => ad.appointment_id == appointment_id && ad.service_id == item.service_id);

                        if (!exists)
                        {
                            var serviceInfo = await _context.Services.FindAsync(item.service_id);
                            _context.AppointmentDetails.Add(new AppointmentDetail
                            {
                                appointment_id = appointment_id,
                                service_id = item.service_id,
                                actual_price = null // Để null để tự động tính theo giá Giảm giá lúc tạo Hóa đơn
                            });
                        }
                    }
                }

                if (Prescriptions != null)
                {
                    foreach (var item in Prescriptions.Where(p => p.medicine_id > 0))
                    {
                        var medication = await _context.Medications.FindAsync(item.medicine_id);
                        if (medication != null)
                        {
                            _context.Prescriptions.Add(new Prescription
                            {
                                record_id = record.record_id,
                                medicine_id = item.medicine_id,
                                quantity = item.quantity,
                                dosage = item.dosage
                            });

                            medication.stock_quantity -= item.quantity;
                        }
                    }
                }

                appointment.status = "Đã khám";

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return RedirectToAction("Index", new { date = appointment.appointment_date.ToString("yyyy-MM-dd") });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return View("Error");
            }
        }

        // 4. Xem lại bệnh án đã hoàn thành
        public async Task<IActionResult> ViewRecord(int id)
        {
            var appointment = await _context.Appointments
                .Include(a => a.pet)
                .Include(a => a.customer)
                .Include(a => a.MedicalRecord)
                    .ThenInclude(m => m.Prescriptions)
                        .ThenInclude(p => p.medicine)
                .Include(a => a.AppointmentDetails)
                    .ThenInclude(ad => ad.service)
                .FirstOrDefaultAsync(a => a.appointment_id == id);

            if (appointment == null || appointment.MedicalRecord == null)
            {
                return NotFound("Không tìm thấy bệnh án cho ca khám này.");
            }

            return View(appointment);
        }
    } // Kết thúc class DoctorController

    // --- Định nghĩa class phụ trợ (nằm ngoài controller nhưng trong namespace) ---
    public class ServiceInput
    {
        public int service_id { get; set; }
        public string? notes { get; set; }
    }

    public class MedicineInput
    {
        public int medicine_id { get; set; }
        public int quantity { get; set; }
        public string? dosage { get; set; }
    }
}