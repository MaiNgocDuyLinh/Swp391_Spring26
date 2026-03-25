using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Group3_SWP391_PetMedical.Services.Interfaces;

namespace Group3_SWP391_PetMedical.Controllers
{
    [Authorize(Roles = "Manager")]
    public class ManagerController : Controller
    {
        private readonly IManagerService _managerService;
        private const int PageSize = 5;

        public ManagerController(IManagerService managerService)
        {
            _managerService = managerService;
        }

        // ========== SERVICES ==========
        public async Task<IActionResult> ListServices(string? search, int page = 1)
        {
            var result = await _managerService.GetServicesPagedAsync(search, page, 5);
            ViewBag.CurrentPage = result.Page;
            ViewBag.TotalPages = result.TotalPages;
            ViewBag.TotalItems = result.TotalItems;
            ViewBag.Search = search;
            return View(result.Items.ToList());
        }

        public async Task<IActionResult> EditService(int id)
        {
            var service = await _managerService.GetServiceByIdAsync(id);
            if (service == null) return NotFound();
            return View(service);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditService(int id, string service_name, decimal base_price,
            string? description, int? duration, bool is_home_service, bool status)
        {
            if (string.IsNullOrWhiteSpace(service_name))
            {
                ModelState.AddModelError("service_name", "Tên dịch vụ không được để trống.");
                var svc = await _managerService.GetServiceByIdAsync(id);
                return View(svc);
            }
            var success = await _managerService.UpdateServiceAsync(id, service_name, base_price, description, duration, is_home_service, status);
            if (!success) return NotFound();
            TempData["SuccessMessage"] = "Cập nhật dịch vụ thành công!";
            return RedirectToAction("ListServices");
        }

        public IActionResult AddService()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddService(string service_name, decimal base_price,
            string? description, int? duration, bool is_home_service)
        {
            if (string.IsNullOrWhiteSpace(service_name))
            {
                ModelState.AddModelError("service_name", "Tên dịch vụ không được để trống.");
                return View();
            }
            await _managerService.CreateServiceAsync(service_name, base_price, description, duration, is_home_service);
            TempData["SuccessMessage"] = "Thêm dịch vụ thành công!";
            return RedirectToAction("ListServices");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteService(int id)
        {
            var success = await _managerService.DeleteServiceAsync(id);
            if (!success)
            {
                TempData["ErrorMessage"] = "Không thể xóa dịch vụ. Dịch vụ đang được sử dụng trong lịch hẹn.";
            }
            else
            {
                TempData["SuccessMessage"] = "Đã xóa dịch vụ thành công!";
            }
            return RedirectToAction("ListServices");
        }

        // ========== APPOINTMENTS ==========
        public async Task<IActionResult> AppointmentList(string? search, string? statusFilter, int page = 1)
        {
            var result = await _managerService.GetAllAppointmentsPagedAsync(search, statusFilter, page, PageSize);
            ViewBag.CurrentPage = result.Page;
            ViewBag.TotalPages = result.TotalPages;
            ViewBag.TotalItems = result.TotalItems;
            ViewBag.Search = search;
            ViewBag.StatusFilter = statusFilter ?? "All";
            return View(result.Items.ToList());
        }

        public async Task<IActionResult> AppointmentDetail(int id)
        {
            var appt = await _managerService.GetAppointmentByIdAsync(id);
            if (appt == null) return NotFound();

            var doctors = await _managerService.GetDoctorsAsync();
            ViewBag.Doctors = doctors;
            return View(appt);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignDoctor(int appointmentId, int doctorId)
        {
            await _managerService.AssignDoctorAsync(appointmentId, doctorId);
            TempData["SuccessMessage"] = "Đã chỉ định bác sĩ thành công!";
            return RedirectToAction("AppointmentDetail", new { id = appointmentId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelAppointment(int id, string? reason)
        {
            await _managerService.CancelAppointmentAsync(id, reason);
            TempData["SuccessMessage"] = "Đã hủy lịch hẹn!";
            return RedirectToAction("AppointmentDetail", new { id });
        }

        // ========== CANCELLED HISTORY ==========
        public async Task<IActionResult> CancelledAppointments(string? search, int page = 1)
        {
            var result = await _managerService.GetCancelledAppointmentsPagedAsync(search, page, PageSize);
            ViewBag.CurrentPage = result.Page;
            ViewBag.TotalPages = result.TotalPages;
            ViewBag.TotalItems = result.TotalItems;
            ViewBag.Search = search;
            return View(result.Items.ToList());
        }

        // ======================= FEEDBACK =======================
        public async Task<IActionResult> FeedbackList(string? search, int? starFilter, int page = 1)
        {
            int PageSize = 6;
            var result = await _managerService.GetFeedbacksPagedAsync(search, starFilter, page, PageSize);
            ViewBag.CurrentPage = result.Page;
            ViewBag.TotalPages = result.TotalPages;
            ViewBag.TotalItems = result.TotalItems;
            ViewBag.Search = search;
            ViewBag.StarFilter = starFilter;
            return View(result.Items.ToList());
        }

        // ======================= SERVICE DISCOUNTS =======================
        public async Task<IActionResult> DiscountList(string? search, int page = 1)
        {
            var result = await _managerService.GetServicesWithDiscountPagedAsync(search, page, PageSize);
            ViewBag.CurrentPage = result.Page;
            ViewBag.TotalPages = result.TotalPages;
            ViewBag.TotalItems = result.TotalItems;
            ViewBag.Search = search;
            return View(result.Items.ToList());
        }

        public async Task<IActionResult> ManageDiscount(int serviceId)
        {
            var service = await _managerService.GetServiceByIdAsync(serviceId);
            if (service == null) return NotFound();

            var activeDiscount = await _managerService.GetActiveDiscountByServiceIdAsync(serviceId);
            ViewBag.ActiveDiscount = activeDiscount;
            return View(service);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApplyDiscount(int serviceId, int discountPercent, DateTime startDate, DateTime endDate)
        {
            if (discountPercent < 1 || discountPercent > 100)
            {
                TempData["ErrorMessage"] = "Phần trăm giảm giá phải từ 1 đến 100.";
                return RedirectToAction("ManageDiscount", new { serviceId });
            }
            if (startDate < DateTime.Now.AddMinutes(-1))
    {
        TempData["ErrorMessage"] = "Thời gian bắt đầu không được ở quá khứ.";
        return RedirectToAction("ManageDiscount", new { serviceId });
    }
    if (endDate <= startDate)
            {
                TempData["ErrorMessage"] = "Ngày kết thúc phải sau ngày bắt đầu.";
                return RedirectToAction("ManageDiscount", new { serviceId });
            }

            var success = await _managerService.ApplyDiscountAsync(serviceId, discountPercent, startDate, endDate);
            if (!success)
            {
                TempData["ErrorMessage"] = "Không thể áp dụng giảm giá. Vui lòng thử lại.";
            }
            else
            {
                TempData["SuccessMessage"] = "Đã áp dụng giảm giá thành công!";
            }
            return RedirectToAction("DiscountList");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveDiscount(int discountId)
        {
            var success = await _managerService.RemoveDiscountAsync(discountId);
            if (!success)
            {
                TempData["ErrorMessage"] = "Không thể hủy giảm giá.";
            }
            else
            {
                TempData["SuccessMessage"] = "Đã hủy giảm giá thành công!";
            }
            return RedirectToAction("DiscountList");
        }

        public async Task<IActionResult> DiscountHistory(string? search, int page = 1)
        {
            var result = await _managerService.GetDiscountHistoryPagedAsync(search, page, PageSize);
            ViewBag.CurrentPage = result.Page;
            ViewBag.TotalPages = result.TotalPages;
            ViewBag.TotalItems = result.TotalItems;
            ViewBag.Search = search;
            return View(result.Items.ToList());
        }
    }
}
