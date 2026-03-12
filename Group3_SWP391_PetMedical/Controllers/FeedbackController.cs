using Group3_SWP391_PetMedical.Services.Interfaces;
using Group3_SWP391_PetMedical.ViewModels.Feedback;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Claims;

namespace Group3_SWP391_PetMedical.Controllers
{
    [Authorize(Roles = "Customer")]
    public class FeedbackController : Controller
    {
        private readonly IFeedbackService _feedbackService;
        private readonly IServiceService _serviceService;

        public FeedbackController(
            IFeedbackService feedbackService,
            IServiceService serviceService)
        {
            _feedbackService = feedbackService;
            _serviceService = serviceService;
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
                TempData["error"] = "Bạn đã đánh giá lịch hẹn này rồi.";
                return RedirectToAction("History", "Feedback");
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
                TempData["error"] = "Bạn đã đánh giá lịch hẹn này rồi.";
                return RedirectToAction("History", "Feedback");
            }

            if (!ModelState.IsValid)
            {
                return View("~/Views/Feedback/CusFeedback.cshtml", vm);
            }

            try
            {
                await _feedbackService.CreateFeedbackAsync(customerId, vm);
                TempData["msg"] = "Gửi phản hồi thành công!";
                return RedirectToAction("History", "Feedback");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View("~/Views/Feedback/CusFeedback.cshtml", vm);
            }
        }

        //history
        [HttpGet]
        public async Task<IActionResult> History([FromQuery] CusFeedbackHistoryQuery filter)
        {
            int customerId = GetCurrentUserId();

            var paged = await _feedbackService.GetCusFeedbackHistoryAsync(customerId, filter);
            var services = await _serviceService.GetAllAsync();

            var vm = new CusFeedbackHistoryListVM
            {
                Filter = filter,
                Page = new()
                {
                    Data = paged,
                    Q = filter.Q
                },
                ServiceOptions = services.Select(s => new SelectListItem
                {
                    Value = s.service_id.ToString(),
                    Text = s.service_name,
                    Selected = filter.ServiceId.HasValue && filter.ServiceId.Value == s.service_id
                }).ToList()
            };

            vm.ServiceOptions.Insert(0, new SelectListItem
            {
                Value = "",
                Text = "Tất cả dịch vụ"
            });

            return View("~/Views/Feedback/CusFeedbackHistory.cshtml", vm);
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