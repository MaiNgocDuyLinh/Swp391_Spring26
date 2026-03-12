using System.Security.Claims;
using Group3_SWP391_PetMedical.Services.Interfaces;
using Group3_SWP391_PetMedical.ViewModels.Feedback;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Group3_SWP391_PetMedical.Controllers
{
    [Authorize(Roles = "Customer")]
    public class FeedbackController : Controller
    {
        private readonly IFeedbackService _feedbackService;

        public FeedbackController(IFeedbackService feedbackService)
        {
            _feedbackService = feedbackService;
        }

        [HttpGet]
        public async Task<IActionResult> Create(int appointmentId)
        {
            int customerId = GetCurrentUserId();

            var vm = await _feedbackService.GetCusCreateFeedbackAsync(customerId, appointmentId);
            if (vm == null) return NotFound();

            var existed = await _feedbackService.HasFeedbackAsync(customerId, appointmentId);
            if (existed)
            {
                TempData["error"] = "Bạn đã feedback lịch hẹn này rồi.";
                return RedirectToAction("AppointmentHistory", "CusAppointment");
            }

            return View("~/Views/Feedback/CusFeedback.cshtml", vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CusCreateFeedbackVM vm)
        {
            int customerId = GetCurrentUserId();

            var current = await _feedbackService.GetCusCreateFeedbackAsync(customerId, vm.AppointmentId);
            if (current == null) return NotFound();

            vm.PetName = current.PetName;
            vm.DoctorName = current.DoctorName;
            vm.ServiceNames = current.ServiceNames;
            vm.AppointmentDate = current.AppointmentDate;

            var existed = await _feedbackService.HasFeedbackAsync(customerId, vm.AppointmentId);
            if (existed)
            {
                TempData["error"] = "Bạn đã feedback lịch hẹn này rồi.";
                return RedirectToAction("AppointmentHistory", "CusAppointment");
            }

            if (!ModelState.IsValid)
            {
                return View("~/Views/Feedback/CusFeedback.cshtml", vm);
            }

            try
            {
                await _feedbackService.CreateFeedbackAsync(customerId, vm);
                TempData["msg"] = "Gửi feedback thành công!";
                return RedirectToAction("AppointmentHistory", "CusAppointment");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View("~/Views/Feedback/CusFeedback.cshtml", vm);
            }
        }

        private int GetCurrentUserId()
        {
            var idStr = User.FindFirstValue(ClaimTypes.NameIdentifier)
                      ?? User.FindFirstValue("UserId");

            if (string.IsNullOrWhiteSpace(idStr) || !int.TryParse(idStr, out var id))
                throw new Exception("Không lấy được customer_id từ Claims. Kiểm tra đăng nhập/claims.");

            return id;
        }
    }
}