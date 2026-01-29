using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Group3_SWP391_PetMedical.Models;
using System.Security.Claims;
using System;
using System.Linq;

namespace Group3_SWP391_PetMedical.Controllers
{
    public class DoctorController : Controller
    {
        private readonly PetClinicContext _context;

        public DoctorController(PetClinicContext context)
        {
            _context = context;
        }

        public IActionResult Index(DateTime? date)
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (claim == null) return RedirectToAction("Login", "Login");

            int doctorId = int.Parse(claim.Value);
            var query = _context.Appointments
                .Include(a => a.pet).ThenInclude(p => p.owner)
                .Where(a => a.doctor_id == doctorId).AsQueryable();

            if (date.HasValue)
                query = query.Where(a => a.appointment_date.Date == date.Value.Date);

            return View(query.OrderBy(a => a.appointment_date).ToList());
        }

        [HttpGet]
        public IActionResult RequestChangeShift() => View();

        [HttpGet]
        public IActionResult GetDoctorsByDate(DateTime date)
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (claim == null) return Json(new { error = "Unauthorized" });

            int currentUserId = int.Parse(claim.Value);

            var doctors = _context.Appointments
                .Include(a => a.doctor)
                .Where(a => a.appointment_date.Date == date.Date && a.doctor_id != currentUserId)
                .Select(a => new { id = a.doctor_id, name = a.doctor.full_name })
                .Distinct().ToList();

            return Json(doctors);
        }

        [HttpPost]
        public IActionResult SubmitShiftRequest(DateTime requestDate, int? targetDoctorId, string reason)
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (claim == null) return RedirectToAction("Login", "Login");
            int userId = int.Parse(claim.Value);

            if (requestDate.Date <= DateTime.Today)
            {
                TempData["ErrorMessage"] = "Yêu cầu phải được gửi trước ít nhất 24 giờ!";
                return RedirectToAction("RequestChangeShift");
            }

            var newRequest = new ShiftChangeRequest
            {
                doctor_id = userId,
                request_date = requestDate,
                reason = targetDoctorId.HasValue
                         ? $"Đổi với BS ID {targetDoctorId}: {reason}"
                         : $"Đổi tự do sang {requestDate:dd/MM/yyyy}: {reason}",
                status = "Pending",
                created_at = DateTime.Now
            };

            _context.ShiftChangeRequests.Add(newRequest);
            _context.SaveChanges();

            TempData["SuccessMessage"] = "Gửi đơn thành công!";
            return RedirectToAction("Index");
        }
    }
}