using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Group3_SWP391_PetMedical.Services.Interfaces;

namespace Group3_SWP391_PetMedical.Controllers
{
    [Authorize(Roles = "Manager")]
    public class ManagerModuleController : Controller
    {
        private readonly IManagerModuleService _managerModuleService;

        public ManagerModuleController(IManagerModuleService managerModuleService)
        {
            _managerModuleService = managerModuleService;
        }

        // ========== YÊU CẦU ĐỔI LỊCH ==========
        public async Task<IActionResult> ListScheduleChangeRequests(string? status)
        {
            var list = await _managerModuleService.GetScheduleChangeRequestsAsync(status ?? "Pending");
            ViewBag.StatusFilter = status ?? "Pending";
            return View("~/Views/Manager/ListScheduleChangeRequests.cshtml", list);
        }

        public async Task<IActionResult> ConfirmScheduleChangeRequest(int id)
        {
            var detail = await _managerModuleService.GetScheduleChangeRequestByIdAsync(id);
            if (detail == null) return NotFound();
            return View("~/Views/Manager/ConfirmScheduleChangeRequest.cshtml", detail);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveScheduleChangeRequest(int id, string? managerNote)
        {
            var userId = int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var uid) ? uid : 0;
            var success = await _managerModuleService.ApproveScheduleChangeRequestAsync(id, userId, managerNote);
            if (!success)
            {
                TempData["error"] = "Không thể chấp nhận yêu cầu (có thể đã xử lý trước đó).";
                return RedirectToAction(nameof(ConfirmScheduleChangeRequest), new { id });
            }
            TempData["SuccessMessage"] = "Đã chấp nhận yêu cầu đổi lịch!";
            return RedirectToAction(nameof(ListScheduleChangeRequests));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectScheduleChangeRequest(int id, string? managerNote)
        {
            var userId = int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var uid) ? uid : 0;
            var success = await _managerModuleService.RejectScheduleChangeRequestAsync(id, userId, managerNote);
            if (!success)
            {
                TempData["error"] = "Không thể từ chối yêu cầu (có thể đã xử lý trước đó).";
                return RedirectToAction(nameof(ConfirmScheduleChangeRequest), new { id });
            }
            TempData["SuccessMessage"] = "Đã từ chối yêu cầu đổi lịch.";
            return RedirectToAction(nameof(ListScheduleChangeRequests));
        }

        // ========== THỐNG KÊ TỔNG QUAN ==========
        public async Task<IActionResult> Overview(DateTime? fromDate, DateTime? toDate, string? groupBy)
        {
            var model = await _managerModuleService.GetOverviewStatsAsync(fromDate, toDate, groupBy);
            return View("~/Views/Manager/Overview.cshtml", model);
        }
    }
}
