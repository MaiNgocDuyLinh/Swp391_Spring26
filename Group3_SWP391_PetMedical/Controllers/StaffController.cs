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

        // ========== SERVICES (view only) ==========
        public async Task<IActionResult> ListServices(string? search, int page = 1)
        {
            var result = await _staffService.GetServicesPagedAsync(search, page, PageSize);
            ViewBag.CurrentPage = result.Page;
            ViewBag.TotalPages = result.TotalPages;
            ViewBag.TotalItems = result.TotalItems;
            ViewBag.Search = search;
            return View(result.Items.ToList());
        }

        // ========== CUSTOMERS ==========
        public async Task<IActionResult> ListCustomers(string? search, int page = 1)
        {
            var result = await _staffService.GetCustomersPagedAsync(search, page, PageSize);
            ViewBag.CurrentPage = result.Page;
            ViewBag.TotalPages = result.TotalPages;
            ViewBag.TotalItems = result.TotalItems;
            ViewBag.Search = search;
            return View(result.Items.ToList());
        }

        // ========== APPOINTMENTS: Lịch theo ngày ==========
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

        // ========== APPOINTMENTS: Xem tất cả + filter ==========
        public async Task<IActionResult> AllAppointments(string? search, string? statusFilter, int page = 1)
        {
            var result = await _staffService.GetAllAppointmentsPagedAsync(search, statusFilter, page, PageSize);
            ViewBag.CurrentPage = result.Page;
            ViewBag.TotalPages = result.TotalPages;
            ViewBag.TotalItems = result.TotalItems;
            ViewBag.Search = search;
            ViewBag.StatusFilter = statusFilter ?? "All";
            return View(result.Items.ToList());
        }

        // ========== APPOINTMENTS: Chi tiết ==========
        public async Task<IActionResult> AppointmentDetail(int id)
        {
            var appt = await _staffService.GetAppointmentByIdAsync(id);
            if (appt == null) return NotFound();

            var doctors = await _staffService.GetDoctorsAsync();
            ViewBag.Doctors = doctors;

            var invoice = await _staffService.GetInvoiceByAppointmentIdAsync(id);
            ViewBag.Invoice = invoice;

            return View(appt);
        }

        // ========== APPOINTMENTS: Approve ==========
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveAppointment(int id)
        {
            await _staffService.ApproveAppointmentAsync(id);
            TempData["SuccessMessage"] = "Đã xác nhận lịch hẹn!";
            return RedirectToAction("AppointmentDetail", new { id });
        }

        // ========== APPOINTMENTS: Reject ==========
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectAppointment(int id, string? reason)
        {
            await _staffService.RejectAppointmentAsync(id, reason);
            TempData["SuccessMessage"] = "Đã hủy lịch hẹn!";
            return RedirectToAction("AppointmentDetail", new { id });
        }

        // ========== APPOINTMENTS: Assign Doctor ==========
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignDoctor(int appointmentId, int doctorId)
        {
            await _staffService.AssignDoctorAsync(appointmentId, doctorId);
            TempData["SuccessMessage"] = "Đã chỉ định bác sĩ thành công!";
            return RedirectToAction("AppointmentDetail", new { id = appointmentId });
        }

        // ========== APPOINTMENTS: Update Status ==========
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, string newStatus)
        {
            await _staffService.UpdateAppointmentStatusAsync(id, newStatus);
            TempData["SuccessMessage"] = $"Đã cập nhật trạng thái thành '{newStatus}'!";
            return RedirectToAction("AppointmentDetail", new { id });
        }

        // ========== INVOICE ==========
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateInvoice(int appointmentId)
        {
            await _staffService.CreateInvoiceAsync(appointmentId);
            TempData["SuccessMessage"] = "Đã tạo hóa đơn thành công!";
            return RedirectToAction("AppointmentDetail", new { id = appointmentId });
        }

        public async Task<IActionResult> ViewInvoice(int appointmentId)
        {
            var invoice = await _staffService.GetInvoiceByAppointmentIdAsync(appointmentId);
            if (invoice == null)
            {
                TempData["ErrorMessage"] = "Chưa có hóa đơn. Vui lòng tạo hóa đơn trước.";
                return RedirectToAction("AppointmentDetail", new { id = appointmentId });
            }
            return View(invoice);
        }
    }
}
