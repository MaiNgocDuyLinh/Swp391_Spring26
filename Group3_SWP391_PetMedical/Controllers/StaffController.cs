using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Group3_SWP391_PetMedical.Services.Interfaces;

namespace Group3_SWP391_PetMedical.Controllers
{
    [Authorize(Roles = "Staff")]
    public class StaffController : Controller
    {
        private readonly IStaffService _staffService;
        private const int PageSize = 5;

        public StaffController(IStaffService staffService)
        {
            _staffService = staffService;
        }

        // ========== 1. VIEW LIST SERVICE (VIEW ONLY) ==========
        public async Task<IActionResult> ListServices(string? search, int page = 1)
        {
            var result = await _staffService.GetServicesPagedAsync(search, page, PageSize);
            
            ViewBag.CurrentPage = result.Page;
            ViewBag.TotalPages = result.TotalPages;
            ViewBag.TotalItems = result.TotalItems;
            ViewBag.Search = search;
            
            return View(result.Items.ToList());
        }

        // ========== 2. VIEW LIST CUSTOMER ==========
        public async Task<IActionResult> ListCustomers(string? search, int page = 1)
        {
            var result = await _staffService.GetCustomersPagedAsync(search, page, PageSize);

            ViewBag.CurrentPage = result.Page;
            ViewBag.TotalPages = result.TotalPages;
            ViewBag.TotalItems = result.TotalItems;
            ViewBag.Search = search;

            return View(result.Items.ToList());
        }

        // ========== 3. STAFF APPOINTMENT LIST ==========
        public async Task<IActionResult> AppointmentList(DateTime? date, string? search, int page = 1)
        {
            var selectedDate = date ?? DateTime.Today;
            ViewBag.SelectedDate = selectedDate;
            ViewBag.Search = search;

            var result = await _staffService.GetAppointmentsByDatePagedAsync(selectedDate, search, page, PageSize);

            ViewBag.CurrentPage = result.Page;
            ViewBag.TotalPages = result.TotalPages;
            ViewBag.TotalItems = result.TotalItems;

            return View(result.Items.ToList());
        }
    }
}
