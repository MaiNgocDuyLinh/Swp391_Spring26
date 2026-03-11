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
    }
}
