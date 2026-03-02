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
                DoctorOptions = doctors.Select(d => new SelectListItem
                {
                    Value = d.DoctorId.ToString(),
                    Text = d.DoctorName
                }).ToList()
            };

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
        // =========================
        [HttpGet]
        public async Task<IActionResult> DoctorShifts(int doctorId, string? day)
        {
            DateTime targetDay;

            if (!string.IsNullOrWhiteSpace(day))
            {
                if (!DateTime.TryParseExact(
                        day.Trim(),
                        "yyyy-MM-dd",
                        CultureInfo.InvariantCulture,
                        DateTimeStyles.None,
                        out targetDay))
                {
                    targetDay = DateTime.Today;
                }
            }
            else
            {
                targetDay = DateTime.Today;
            }

            targetDay = targetDay.Date;

            var shifts = await _cusAppointmentService.GetDoctorShiftsAsync(doctorId, targetDay, targetDay);

            return Json(shifts.Select(x => new
            {
                start = x.Start,
                end = x.End,
                display = x.Display
            }));
        }

        // =========================
        // ✅ DETAILS (popup)
        // /CusAppointment/Details?id=44&popup=1
        // =========================
        [HttpGet]
        public async Task<IActionResult> Details(int id, int popup = 0)
        {
            int customerId = GetCurrentUserId();

            var vm = await _cusAppointmentService.GetCusAppointmentDetailAsync(customerId, id);
            if (vm == null) return NotFound();

            if (popup == 1) ViewBag.Popup = true;

            return View("~/Views/Appointment/CusAppointmentDetails.cshtml", vm);
        }

        // =========================
        // ✅ CANCEL (GET popup)  <<< FIX CHÍNH Ở ĐÂY
        // /CusAppointment/Cancel?id=44&popup=1
        // =========================
        [HttpGet]
        public async Task<IActionResult> Cancel(int id, int popup = 0)
        {
            int customerId = GetCurrentUserId();

            // ✅ LẤY ĐÚNG DATA (Ngày giờ / Thú cưng / Dịch vụ / Mô tả)
            var vm = await _cusAppointmentService.GetCusCancelAppointmentAsync(customerId, id);

            if (vm == null)
                return NotFound();

            if (popup == 1) ViewBag.Popup = true;

            return View("~/Views/Appointment/CusCancelAppointment.cshtml", vm);
        }

        // POST: /CusAppointment/Cancel
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(CusCancelAppointmentVM vm, int popup = 0)
        {
            int customerId = GetCurrentUserId();

            if (string.IsNullOrWhiteSpace(vm.Reason))
                ModelState.AddModelError(nameof(vm.Reason), "Vui lòng nhập lý do hủy.");

            if (!ModelState.IsValid)
            {
                // reload tóm tắt để không bị trống khi validate fail
                var reload = await _cusAppointmentService.GetCusCancelAppointmentAsync(customerId, vm.AppointmentId);
                if (reload != null)
                {
                    vm.AppointmentDate = reload.AppointmentDate;
                    vm.PetName = reload.PetName;
                    vm.ServiceNames = reload.ServiceNames;
                    vm.Description = reload.Description;
                }

                if (popup == 1) ViewBag.Popup = true;
                return View("~/Views/Appointment/CusCancelAppointment.cshtml", vm);
            }

            var ok = await _cusAppointmentService
     .CancelCusAppointmentAsync(customerId, vm.AppointmentId, vm.Reason);

            if (!ok) return NotFound();

            // ✅ Nếu là popup → chuyển parent về MyAppointments (reload luôn)
            if (popup == 1)
            {
                var backUrl = Url.Action("MyAppointments", "CusAppointment");

                return Content($@"
                                    <!doctype html>
                                    <html>
                                    <head><meta charset='utf-8'></head>
                                    <body>
                                    <script>
                                        window.parent.location.href = '{backUrl}';
                                    </script>
                                    </body>
                                    </html>
                                    ", "text/html");
            }

            // không popup
            return RedirectToAction(nameof(MyAppointments));
        }

        // =========================
        // helper: reload options for Book view
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