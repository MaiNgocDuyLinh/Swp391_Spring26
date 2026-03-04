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

        //  GET: /CusAppointment/Book
        //  Dùng cho:
        //  - Lần đầu mở form
        //  - Bấm nút "Xem lịch bác sĩ" (submit GET với các field của Form)
        [HttpGet]
        public async Task<IActionResult> Book(
            [FromQuery(Name = "Form.DoctorId")] int? doctorId,
            [FromQuery(Name = "Form.AppointmentDate")] DateTime? appointmentDate,
            [FromQuery(Name = "Form.PetId")] int? petId,
            [FromQuery(Name = "Form.ServiceIds")] List<int>? serviceIds,
            [FromQuery(Name = "Form.Notes")] string? notes)
        {
            int customerId = GetCurrentUserId();

            var pets = await _cusAppointmentService.GetCustomerPetsAsync(customerId);
            var services = await _serviceService.GetAllAsync();
            var doctors = await _cusAppointmentService.GetDoctorsAsync();

            var vm = new CusCreateAppointmentVM();

            if (petId.HasValue)
            {
                vm.Form.PetId = petId.Value;
            }
            vm.Form.DoctorId = doctorId;
            vm.Form.AppointmentDate = appointmentDate ?? default;
            vm.Form.Notes = notes;
            vm.Form.ServiceIds = serviceIds ?? new List<int>();

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

            if (vm.Form.DoctorId.HasValue && appointmentDate.HasValue)
            {
                var day = appointmentDate.Value.Date;
                vm.DoctorShifts = await _cusAppointmentService.GetDoctorShiftsAsync(vm.Form.DoctorId.Value, day, day);
            }

            return View("~/Views/Appointment/CusBookAppointment.cshtml", vm);
        }

        //  POST: /CusAppointment/Book
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

        // get doctorshift
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


        //   Get full calendar of doctor (busy appointments) 
        // GET: /CusAppointment/DoctorCalendar?doctorId=5&from=2026-03-01&to=2026-03-15
        [HttpGet]
        public async Task<IActionResult> DoctorCalendar(int doctorId, string? from, string? to)
        {
            DateTime fromDate;
            DateTime toDate;

            // parse yyyy-MM-dd
            if (!DateTime.TryParseExact(from ?? "",
                    "yyyy-MM-dd",
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out fromDate))
            {
                fromDate = DateTime.Today;
            }

            if (!DateTime.TryParseExact(to ?? "",
                    "yyyy-MM-dd",
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out toDate))
            {
                toDate = DateTime.Today.AddDays(14);
            }

            fromDate = fromDate.Date;
            toDate = toDate.Date.AddDays(1).AddTicks(-1);

            // Lấy toàn bộ lịch hẹn của bác sĩ trong khoảng thời gian
            var appointments = await _cusAppointmentService
                .GetDoctorAppointmentsAsync(doctorId, fromDate, toDate);

            // Trả về format chuẩn cho FullCalendar
            return Json(appointments.Select(a => new
            {
                title = a.Title,
                start = a.Start,
                end = a.End,
                status = a.Status
            }));
        }
        //get details
        [HttpGet]
        public async Task<IActionResult> Details(int id, int? popup)
        {
            int customerId = GetCurrentUserId();

            var vm = await _cusAppointmentService.GetCusAppointmentDetailAsync(customerId, id);
            if (vm == null) return NotFound();

            if (popup == 1) ViewBag.IsPopup = true;

            return View("~/Views/Appointment/CusAppointmentDetails.cshtml", vm);
        }

        //  EDIT Get
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            int customerId = GetCurrentUserId();

            var vm = await _cusAppointmentService.GetCusEditAppointmentAsync(customerId, id);
            if (vm == null) return NotFound();

            if (vm.CreatedAt != default && DateTime.Now > vm.CreatedAt.AddHours(24))
            {
                TempData["error"] = "Không thể thay đổi lịch hẹn sau 24h.";
                return RedirectToAction(nameof(MyAppointments));
            }

            var services = await _serviceService.GetAllAsync();
            ViewBag.ServiceOptions = services.Select(s => new SelectListItem
            {
                Value = s.service_id.ToString(),
                Text = s.service_name,
                Selected = vm.ServiceIds != null && vm.ServiceIds.Contains(s.service_id)
            }).ToList();

            return View("~/Views/Appointment/CusEditAppointment.cshtml", vm);
        }
        //  EDIT POST  
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(CusEditAppointmentVM vm)
        {
            int customerId = GetCurrentUserId();

            //  đảm bảo list không null
            vm.ServiceIds ??= new List<int>();

            //  Lấy lại bản ghi hiện tại từ DB để:
            // - lấy CreatedAt thật (form không submit CreatedAt)
            // - chống bypass 24h
            var current = await _cusAppointmentService.GetCusEditAppointmentAsync(customerId, vm.AppointmentId);
            if (current == null) return NotFound();

            //  gán lại CreatedAt/Status để view hiển thị đúng nếu trả về View do lỗi
            vm.CreatedAt = current.CreatedAt;
            vm.Status = current.Status;

            //  CHẶN SAU 24H (DÙNG current.CreatedAt)
            if (current.CreatedAt != default && DateTime.Now > current.CreatedAt.AddHours(24))
            {
                TempData["error"] = "Không thể thay đổi lịch hẹn sau 24h.";
                return RedirectToAction(nameof(MyAppointments));
            }

            //  Bắt buộc chọn ít nhất 1 dịch vụ
            if (vm.ServiceIds.Count == 0)
            {
                ModelState.AddModelError("ServiceIds", "Vui lòng chọn ít nhất 1 dịch vụ.");
            }

            if (!ModelState.IsValid)
            {
                var services = await _serviceService.GetAllAsync();
                ViewBag.ServiceOptions = services.Select(s => new SelectListItem
                {
                    Value = s.service_id.ToString(),
                    Text = s.service_name,
                    Selected = vm.ServiceIds.Contains(s.service_id)
                }).ToList();

                return View("~/Views/Appointment/CusEditAppointment.cshtml", vm);
            }

            try
            {
                var ok = await _cusAppointmentService.UpdateCusAppointmentAsync(customerId, vm);
                if (!ok) return NotFound();

                TempData["msg"] = "Cập nhật lịch hẹn thành công!";
                return RedirectToAction(nameof(MyAppointments));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);

                var services = await _serviceService.GetAllAsync();
                ViewBag.ServiceOptions = services.Select(s => new SelectListItem
                {
                    Value = s.service_id.ToString(),
                    Text = s.service_name,
                    Selected = vm.ServiceIds.Contains(s.service_id)
                }).ToList();

                return View("~/Views/Appointment/CusEditAppointment.cshtml", vm);
            }
        }

        // CANCEL GET (popup)
        // /CusAppointment/Cancel?id=34&popup=1
        [HttpGet]
        public async Task<IActionResult> Cancel(int id, int? popup)
        {
            int customerId = GetCurrentUserId();

            var detail = await _cusAppointmentService.GetCusAppointmentDetailAsync(customerId, id);
            if (detail == null) return NotFound();

            var vm = new CusCancelAppointmentVM
            {
                AppointmentId = detail.AppointmentId,
                AppointmentDate = detail.AppointmentDate,
                PetName = detail.PetName,
                ServiceNames = (detail.Services != null ? string.Join(", ", detail.Services) : ""),
                Reason = ""
            };

            if (popup == 1) ViewBag.IsPopup = true;

            return View("~/Views/Appointment/CusCancelAppointment.cshtml", vm);
        }

        //  CANCEL POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(CusCancelAppointmentVM vm, int? popup)
        {
            int customerId = GetCurrentUserId();

            if (string.IsNullOrWhiteSpace(vm.Reason))
            {
                ModelState.AddModelError(nameof(vm.Reason), "Vui lòng nhập lý do hủy.");
            }

            if (!ModelState.IsValid)
            {
                if (popup == 1) ViewBag.IsPopup = true;
                return View("~/Views/Appointment/CusCancelAppointment.cshtml", vm);
            }

            var ok = await _cusAppointmentService.CancelCusAppointmentAsync(customerId, vm.AppointmentId, vm.Reason);
            if (!ok) return NotFound();

            TempData["msg"] = "Đã hủy lịch hẹn thành công!";

            //  Sau khi hủy: luôn quay về lịch sử khám bệnh
            if (popup == 1)
            {
                // URL trang cha cần quay về
                ViewBag.GoBackUrl = Url.Action(nameof(AppointmentHistory), "CusAppointment");
                return View("~/Views/Appointment/_PopupRedirectParent.cshtml");
            }

            return RedirectToAction(nameof(AppointmentHistory));
        }

        // helper: reload options for Book view
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