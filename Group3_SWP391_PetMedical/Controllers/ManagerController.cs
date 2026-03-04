using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Group3_SWP391_PetMedical.Models;
using Group3_SWP391_PetMedical.Services.Interfaces;
using Group3_SWP391_PetMedical.ViewModels.Manager;

namespace Group3_SWP391_PetMedical.Controllers
{
    [Authorize(Roles = "Manager")]
    public class ManagerController : Controller
    {
        private readonly IManagerModuleService _managerService;
        private readonly PetClinicContext _context;
        private const int PageSize = 5;

        public ManagerController(IManagerModuleService managerService, PetClinicContext context)
        {
            _managerService = managerService;
            _context = context;
        }

        private int? GetCurrentUserId()
        {
            var idStr = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("user_id");
            return int.TryParse(idStr, out var id) ? id : null;
        }

        // ========== 1. VIEW LIST SERVICE ==========
        public async Task<IActionResult> ListServices(string? search, int page = 1)
        {
            var result = await _managerService.GetServicesPagedAsync(search, page, PageSize);
            
            ViewBag.CurrentPage = result.Page;
            ViewBag.TotalPages = result.TotalPages;
            ViewBag.TotalItems = result.TotalItems;
            ViewBag.Search = search;
            
            return View(result.Items.ToList());
        }

        // ========== 2. EDIT SERVICE (GET) ==========
        public async Task<IActionResult> EditService(int id)
        {
            var service = await _managerService.GetServiceByIdAsync(id);
            if (service == null)
            {
                return NotFound();
            }
            return View(service);
        }

        // ========== 2. EDIT SERVICE (POST) ==========
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditService(int id, decimal base_price, string? description)
        {
            var success = await _managerService.UpdateServiceAsync(id, base_price, description);
            if (!success)
            {
                return NotFound();
            }

            TempData["SuccessMessage"] = "Cập nhật dịch vụ thành công!";
            return RedirectToAction("ListServices");
        }

        // ========== 3. OVERVIEW STATISTICS (Thống kê tổng quan) ==========
        public async Task<IActionResult> Overview(DateTime? fromDate, DateTime? toDate, string? groupBy)
        {
            var now = DateTime.Now;
            var start = fromDate ?? new DateTime(now.Year, now.Month, 1);
            var end = toDate ?? now.Date.AddDays(1).AddTicks(-1);
            if (end < start) end = start;
            groupBy = string.IsNullOrEmpty(groupBy) || groupBy != "month" ? "day" : "month";

            var vm = new OverviewStatsVM
            {
                FromDate = start,
                ToDate = end,
                GroupBy = groupBy
            };

            // Doanh thu: tổng từ Invoices đã thanh toán (Paid/Completed) trong kỳ
            var paidStatuses = new[] { "Paid", "Completed", "paid", "completed" };
            vm.Revenue = await _context.Invoices
                .Where(i => i.created_at >= start && i.created_at <= end &&
                            i.payment_status != null && paidStatuses.Contains(i.payment_status))
                .SumAsync(i => i.total_amount);

            vm.CustomerLoginCount = 0; // Chức năng ghi log đăng nhập đã bỏ

            // Số lịch khám đã đặt trong kỳ (theo created_at)
            var appointmentsInRange = await _context.Appointments
                .Where(a => a.created_at >= start && a.created_at <= end)
                .ToListAsync();
            vm.TotalAppointments = appointmentsInRange.Count;
            vm.AppointmentsByStatus = appointmentsInRange
                .GroupBy(a => a.status ?? "Khác")
                .ToDictionary(g => g.Key, g => g.Count());

            // Dữ liệu biểu đồ: doanh thu theo ngày/tháng
            var invoicesForChart = await _context.Invoices
                .Where(i => i.created_at >= start && i.created_at <= end &&
                            i.payment_status != null && paidStatuses.Contains(i.payment_status))
                .Select(i => new { i.created_at, i.total_amount })
                .ToListAsync();
            if (groupBy == "month")
            {
                vm.RevenueByDate = invoicesForChart
                    .GroupBy(i => new { Year = i.created_at?.Year ?? 0, Month = i.created_at?.Month ?? 0 })
                    .OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Month)
                    .Select(g => new RevenueByDateItem
                    {
                        Label = $"{g.Key.Month:D2}/{g.Key.Year}",
                        Value = g.Sum(x => x.total_amount)
                    })
                    .ToList();
            }
            else
            {
                vm.RevenueByDate = invoicesForChart
                    .GroupBy(i => i.created_at?.Date ?? DateTime.MinValue)
                    .OrderBy(g => g.Key)
                    .Select(g => new RevenueByDateItem
                    {
                        Label = g.Key.ToString("dd/MM"),
                        Value = g.Sum(x => x.total_amount)
                    })
                    .ToList();
            }

            // Số lịch khám theo ngày/tháng cho biểu đồ
            var apptsForChart = await _context.Appointments
                .Where(a => a.created_at >= start && a.created_at <= end)
                .Select(a => a.created_at)
                .ToListAsync();
            if (groupBy == "month")
            {
                vm.AppointmentsByDate = apptsForChart
                    .GroupBy(d => d.HasValue ? new DateTime(d.Value.Year, d.Value.Month, 1) : DateTime.MinValue)
                    .Where(g => g.Key != DateTime.MinValue)
                    .OrderBy(g => g.Key)
                    .Select(g => new AppointmentsByDateItem { Label = g.Key.ToString("MM/yyyy"), Count = g.Count() })
                    .ToList();
            }
            else
            {
                vm.AppointmentsByDate = apptsForChart
                    .Where(d => d.HasValue)
                    .GroupBy(d => d!.Value.Date)
                    .OrderBy(g => g.Key)
                    .Select(g => new AppointmentsByDateItem { Label = g.Key.ToString("dd/MM"), Count = g.Count() })
                    .ToList();
            }

            return View(vm);
        }

        // ========== 4. CONFIRM JOB CHANGE REQUEST (Yêu cầu đổi lịch làm việc bác sĩ) ==========
        /// <summary>Danh sách yêu cầu đổi lịch (mặc định: trạng thái Chờ duyệt).</summary>
        public async Task<IActionResult> ListScheduleChangeRequests(string? status)
        {
            // Lần đầu vào trang: mặc định "Pending". Chọn "Tất cả" gửi status="" -> hiển thị tất cả.
            var filterStatus = status == null ? "Pending" : (string.IsNullOrWhiteSpace(status) ? null : status.Trim());
            var list = await _managerService.GetScheduleChangeRequestsAsync(filterStatus);
            ViewBag.FilterStatus = filterStatus ?? "";
            return View(list);
        }

        /// <summary>Chi tiết yêu cầu - trang Chấp nhận / Từ chối.</summary>
        public async Task<IActionResult> ConfirmScheduleChangeRequest(int id)
        {
            var detail = await _managerService.GetScheduleChangeRequestByIdAsync(id);
            if (detail == null) return NotFound();
            if (!detail.Status.Equals("Pending", StringComparison.OrdinalIgnoreCase))
            {
                TempData["InfoMessage"] = "Yêu cầu này đã được xử lý.";
                return RedirectToAction(nameof(ListScheduleChangeRequests));
            }
            return View(detail);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveScheduleChangeRequest(int id, string? managerNote)
        {
            var managerId = GetCurrentUserId();
            if (managerId == null)
            {
                TempData["error"] = "Không xác định được Manager.";
                return RedirectToAction(nameof(ConfirmScheduleChangeRequest), new { id });
            }
            var ok = await _managerService.ApproveScheduleChangeRequestAsync(id, managerId.Value, managerNote);
            if (!ok)
            {
                TempData["error"] = "Không thể chấp nhận yêu cầu (đã xử lý hoặc không tồn tại).";
                return RedirectToAction(nameof(ListScheduleChangeRequests));
            }
            TempData["SuccessMessage"] = "Đã chấp nhận yêu cầu đổi lịch.";
            return RedirectToAction(nameof(ListScheduleChangeRequests));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectScheduleChangeRequest(int id, string? managerNote)
        {
            var managerId = GetCurrentUserId();
            if (managerId == null)
            {
                TempData["error"] = "Không xác định được Manager.";
                return RedirectToAction(nameof(ConfirmScheduleChangeRequest), new { id });
            }
            var ok = await _managerService.RejectScheduleChangeRequestAsync(id, managerId.Value, managerNote);
            if (!ok)
            {
                TempData["error"] = "Không thể từ chối yêu cầu (đã xử lý hoặc không tồn tại).";
                return RedirectToAction(nameof(ListScheduleChangeRequests));
            }
            TempData["SuccessMessage"] = "Đã từ chối yêu cầu đổi lịch.";
            return RedirectToAction(nameof(ListScheduleChangeRequests));
        }
    }
}
