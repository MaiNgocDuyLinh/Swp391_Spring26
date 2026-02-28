using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Claims;
using Group3_SWP391_PetMedical.Services.Interfaces;
using Group3_SWP391_PetMedical.ViewModels.Appointment;
using System.Globalization;

namespace Group3_SWP391_PetMedical.Controllers
{
    [Authorize(Roles = "Customer")]
    public class CusAppointmentController : Controller
    {
        private readonly ICusAppointmentService _cusAppointmentService;
        private readonly IServiceService _serviceService;

        public CusAppointmentController(
            ICusAppointmentService cusAppointmentService,
            IServiceService serviceService)
        {
            _cusAppointmentService = cusAppointmentService;
            _serviceService = serviceService;
        }

        // GET: /CusAppointment/AppointmentHistory
        [HttpGet]
        public async Task<IActionResult> AppointmentHistory([FromQuery] CusAppointmentHistoryQuery filter)
        {
            int customerId = GetCurrentUserId();

            var paged = await _cusAppointmentService
                .GetCusAppointmentHistoryAsync(customerId, filter);

            var services = await _serviceService.GetAllAsync();

            var vm = new CusAppointmentHistoryListVM
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

            return View("~/Views/Appointment/CusAppointmentHistory.cshtml", vm);
        }

        // GET: /CusAppointment/MyAppointments
        [HttpGet]
        public async Task<IActionResult> MyAppointments([FromQuery] CusBookedAppointmentQuery filter)
        {
            int customerId = GetCurrentUserId();

            var paged = await _cusAppointmentService
                .GetCusBookedAppointmentsAsync(customerId, filter);

            var services = await _serviceService.GetAllAsync();

            var vm = new CusBookedAppointmentListVM
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

            return View("~/Views/Appointment/CusMyAppointments.cshtml", vm);
        }

        // =========================
        // ✅ GET: /CusAppointment/Book
        // =========================
        [HttpGet]
        public async Task<IActionResult> Book()
        {
            int customerId = GetCurrentUserId();

            var pets = await _cusAppointmentService.GetCustomerPetsAsync(customerId);
            var services = await _serviceService.GetAllAsync();

            // ✅ NEW: doctors
            var doctors = await _cusAppointmentService.GetDoctorsAsync();

            var vm = new CusCreateAppointmentVM
            {
                PetOptions = pets.Select(p => new SelectListItem
                {
                    Value = p.PetId.ToString(),
                    Text = p.PetName
                }).ToList(),
                ServiceOptions = services.Select(s => new SelectListItem
                {
                    Value = s.service_id.ToString(),
                    Text = s.service_name
                }).ToList(),

                // ✅ NEW: doctor dropdown
                DoctorOptions = doctors.Select(d => new SelectListItem
                {
                    Value = d.DoctorId.ToString(),
                    Text = d.DoctorName
                }).ToList()
            };

            // option đầu: chưa phân công
            vm.DoctorOptions.Insert(0, new SelectListItem
            {
                Value = "",
                Text = "Chưa phân công",
                Selected = true
            });

            return View("~/Views/Appointment/CusBookAppointment.cshtml", vm);
        }

        // =========================
        // ✅ POST: /CusAppointment/Book
        // =========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Book(CusCreateAppointmentVM vm)
        {
            int customerId = GetCurrentUserId();

            // đảm bảo list không null
            vm.Form.ServiceIds ??= new List<int>();

            if (!ModelState.IsValid)
            {
                await ReloadBookOptions(customerId, vm);
                return View("~/Views/Appointment/CusBookAppointment.cshtml", vm);
            }

            try
            {
                var newId = await _cusAppointmentService.CreateAppointmentAsync(customerId, vm.Form);
                TempData["msg"] = $"Đặt lịch thành công! Mã lịch: #{newId}";
                return RedirectToAction(nameof(MyAppointments));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);

                await ReloadBookOptions(customerId, vm);
                return View("~/Views/Appointment/CusBookAppointment.cshtml", vm);
            }
        }

        // =========================
        // ✅ NEW: /CusAppointment/DoctorShifts?doctorId=3&day=2026-03-01
        // Trả JSON ca làm để view hiển thị và check khớp giờ đặt
        // =========================
        [HttpGet]
        public async Task<IActionResult> DoctorShifts(int doctorId, string? day)
        {
            // ✅ luôn parse theo ISO yyyy-MM-dd để tránh lệch culture
            DateTime targetDay;

            if (!string.IsNullOrWhiteSpace(day))
            {
                // Parse exact yyyy-MM-dd (vd: 2026-03-05)
                if (!DateTime.TryParseExact(
                        day.Trim(),
                        "yyyy-MM-dd",
                        CultureInfo.InvariantCulture,
                        DateTimeStyles.None,
                        out targetDay))
                {
                    // parse fail -> fallback today
                    targetDay = DateTime.Today;
                }
            }
            else
            {
                targetDay = DateTime.Today;
            }

            targetDay = targetDay.Date;

            // ✅ nếu service của bạn cần from/to, truyền cùng ngày như bạn đang làm
            var shifts = await _cusAppointmentService.GetDoctorShiftsAsync(doctorId, targetDay, targetDay);

            return Json(shifts.Select(x => new
            {
                start = x.Start,      // DateTime
                end = x.End,          // DateTime
                display = x.Display   // string
            }));
        }

        // =========================
        // ✅ NEW helper: reload options for Book view
        // =========================
        private async Task ReloadBookOptions(int customerId, CusCreateAppointmentVM vm)
        {
            var pets = await _cusAppointmentService.GetCustomerPetsAsync(customerId);
            var services = await _serviceService.GetAllAsync();
            var doctors = await _cusAppointmentService.GetDoctorsAsync();

            vm.PetOptions = pets.Select(p => new SelectListItem
            {
                Value = p.PetId.ToString(),
                Text = p.PetName,
                Selected = (p.PetId == vm.Form.PetId)
            }).ToList();

            vm.ServiceOptions = services.Select(s => new SelectListItem
            {
                Value = s.service_id.ToString(),
                Text = s.service_name,
                Selected = vm.Form.ServiceIds.Contains(s.service_id)
            }).ToList();

            vm.DoctorOptions = doctors.Select(d => new SelectListItem
            {
                Value = d.DoctorId.ToString(),
                Text = d.DoctorName,
                Selected = vm.Form.DoctorId.HasValue && vm.Form.DoctorId.Value == d.DoctorId
            }).ToList();

            vm.DoctorOptions.Insert(0, new SelectListItem
            {
                Value = "",
                Text = "Chưa phân công",
                Selected = !vm.Form.DoctorId.HasValue
            });
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