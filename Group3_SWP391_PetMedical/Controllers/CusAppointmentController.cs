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

        // ==========================================================
        // LỊCH SỬ
        // ==========================================================
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

        // ==========================================================
        // LỊCH ĐÃ ĐẶT
        // ==========================================================
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

        // ==========================================================
        // CHI TIẾT LỊCH HẸN
        // ==========================================================
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            int customerId = GetCurrentUserId();

            var detail = await _cusAppointmentService
                .GetCusAppointmentDetailAsync(customerId, id);

            if (detail == null)
                return NotFound();

            // ✅ Normalize status để không lỗi int vs string
            var statusKey = NormalizeStatus(detail.Status);

            var within24h = (DateTime.Now - detail.CreatedAt) <= TimeSpan.FromHours(24);

            detail.CanEdit = within24h &&
                             statusKey != "đã hủy" &&
                             statusKey != "đã thanh toán" &&
                             statusKey != "không đến";

            detail.CanCancel = statusKey != "đã hủy" &&
                               statusKey != "đã thanh toán" &&
                               statusKey != "không đến";

            return View("~/Views/Appointment/CusAppointmentDetails.cshtml", detail);
        }

        // ==========================================================
        // CHỈNH SỬA LỊCH HẸN (<= 24h từ CreatedAt, KHÔNG sửa status)
        // ==========================================================
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            int customerId = GetCurrentUserId();

            var vm = await _cusAppointmentService
                .GetCusEditAppointmentAsync(customerId, id);

            if (vm == null)
                return NotFound();

            var statusKey = NormalizeStatus(vm.Status);
            var within24h = (DateTime.Now - vm.CreatedAt) <= TimeSpan.FromHours(24);

            if (!within24h ||
                statusKey == "đã hủy" ||
                statusKey == "đã thanh toán" ||
                statusKey == "không đến")
            {
                TempData["Err"] = "Chỉ được chỉnh sửa trong vòng 24h từ lúc tạo lịch và khi lịch chưa hủy / chưa thanh toán.";
                return RedirectToAction(nameof(Details), new { id });
            }

            vm.ServiceIds ??= new List<int>();

            var services = await _serviceService.GetAllAsync();
            vm.AllServices = services.Select(s => new SelectListItem
            {
                Value = s.service_id.ToString(),
                Text = s.service_name,
                Selected = vm.ServiceIds.Contains(s.service_id)
            }).ToList();

            return View("~/Views/Appointment/CusEditAppointment.cshtml", vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(CusEditAppointmentVM vm)
        {
            int customerId = GetCurrentUserId();

            vm.ServiceIds ??= new List<int>();

            // ✅ check lại rule từ DB cho chắc
            var detail = await _cusAppointmentService
                .GetCusAppointmentDetailAsync(customerId, vm.AppointmentId);

            if (detail == null)
                return NotFound();

            var statusKey = NormalizeStatus(detail.Status);
            var within24h = (DateTime.Now - detail.CreatedAt) <= TimeSpan.FromHours(24);

            if (!within24h ||
                statusKey == "đã hủy" ||
                statusKey == "đã thanh toán" ||
                statusKey == "không đến")
            {
                TempData["Err"] = "Không thể chỉnh sửa lịch hẹn này.";
                return RedirectToAction(nameof(Details), new { id = vm.AppointmentId });
            }

            if (!ModelState.IsValid)
            {
                var services = await _serviceService.GetAllAsync();
                vm.AllServices = services.Select(s => new SelectListItem
                {
                    Value = s.service_id.ToString(),
                    Text = s.service_name,
                    Selected = vm.ServiceIds.Contains(s.service_id)
                }).ToList();

                return View("~/Views/Appointment/CusEditAppointment.cshtml", vm);
            }

            await _cusAppointmentService.UpdateCusAppointmentAsync(customerId, vm);

            TempData["Ok"] = "Cập nhật lịch hẹn thành công.";
            return RedirectToAction(nameof(Details), new { id = vm.AppointmentId });
        }

        // ==========================================================
        // HỦY LỊCH HẸN (hiện mô tả + nhập lý do)
        // ==========================================================
        [HttpGet]
        public async Task<IActionResult> Cancel(int id)
        {
            int customerId = GetCurrentUserId();

            var detail = await _cusAppointmentService
                .GetCusAppointmentDetailAsync(customerId, id);

            if (detail == null)
                return NotFound();

            var statusKey = NormalizeStatus(detail.Status);

            if (statusKey == "đã hủy" ||
                statusKey == "đã thanh toán" ||
                statusKey == "không đến")
            {
                TempData["Err"] = "Không thể hủy lịch hẹn này.";
                return RedirectToAction(nameof(Details), new { id });
            }

            var vm = new CusCancelAppointmentVM
            {
                AppointmentId = id,
                Description = detail.Notes
            };

            return View("~/Views/Appointment/CusCancelAppointment.cshtml", vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(CusCancelAppointmentVM vm)
        {
            int customerId = GetCurrentUserId();

            if (string.IsNullOrWhiteSpace(vm.Reason))
            {
                ModelState.AddModelError(nameof(vm.Reason), "Vui lòng nhập lý do hủy.");
                return View("~/Views/Appointment/CusCancelAppointment.cshtml", vm);
            }

            await _cusAppointmentService.CancelCusAppointmentAsync(customerId, vm.AppointmentId, vm.Reason.Trim());

            TempData["Ok"] = "Đã hủy lịch hẹn.";
            return RedirectToAction(nameof(Details), new { id = vm.AppointmentId });
        }

        // ==========================================================
        // BOOK APPOINTMENT (GIỮ NGUYÊN LOGIC CŨ)
        // ==========================================================
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

        /// <summary>
        /// Chuẩn hóa Status để tránh lỗi int/string.
        /// - Nếu status là string: trim + lower
        /// - Nếu status là int: map theo quy ước (nếu dự án bạn dùng status code)
        ///   Bạn có thể chỉnh mapping ở đây nếu khác.
        /// </summary>
        private static string NormalizeStatus(object? status)
        {
            if (status == null) return "";

            if (status is string s)
                return (s ?? "").Trim().ToLower();

            if (status is int i)
            {
                // ⚠️ Nếu dự án bạn đang dùng status code, chỉnh map tại đây
                // Mặc định: 2=đã thanh toán, 3=đã hủy, 4=không đến (bạn sửa theo hệ của bạn)
                return i switch
                {
                    2 => "đã thanh toán",
                    3 => "đã hủy",
                    4 => "không đến",
                    _ => i.ToString()
                };
            }

            // fallback
            return status.ToString()?.Trim().ToLower() ?? "";
        }
    }
}