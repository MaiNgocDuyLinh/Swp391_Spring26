using Group3_SWP391_PetMedical.Data;
using Group3_SWP391_PetMedical.Models;
using Group3_SWP391_PetMedical.Models.Common;
using Group3_SWP391_PetMedical.Repository.Interfaces;
using Group3_SWP391_PetMedical.ViewModels.Appointment;
using Microsoft.EntityFrameworkCore;

namespace Group3_SWP391_PetMedical.Repository.Implementations
{
    public class CusAppointmentRepository : ICusAppointmentRepository
    {
        private readonly PetClinicContext _context;

        // ⚠️ ĐỔI TÊN FIELD này đúng với cột "ngày tạo" thật trong Appointments của bạn
        // Ví dụ: "CreatedAt", "created_date", "createdAt", ...
        private const string CREATED_AT_FIELD = "created_at";

        public CusAppointmentRepository(PetClinicContext context)
        {
            _context = context;
        }

        public async Task<PagedResult<CusAppointmentHistoryItemVM>>
            GetCusAppointmentHistoryAsync(int customerId, CusAppointmentHistoryQuery query)
        {
            var q = _context.Appointments
                .AsNoTracking()
                .Where(a => a.customer_id == customerId);

            // Lịch sử: chỉ lấy lịch có trạng thái "đã thanh toán" hoặc "đã hủy" , "không đến "
            q = q.Where(a => a.status != null &&
                (a.status.Trim().ToLower() == "đã thanh toán" ||
                 a.status.Trim().ToLower() == "đã hủy" ||
                 a.status.Trim().ToLower() == "không đến"));

            // Filter ngày
            if (query.FromDate.HasValue)
                q = q.Where(a => a.appointment_date >= query.FromDate.Value);

            if (query.ToDate.HasValue)
                q = q.Where(a => a.appointment_date <= query.ToDate.Value);

            // Filter theo dịch vụ
            if (query.ServiceId.HasValue)
            {
                q = q.Where(a => a.AppointmentDetails.Any(d => d.service_id == query.ServiceId.Value));
            }

            // Search keyword
            if (!string.IsNullOrWhiteSpace(query.Q))
            {
                var kw = query.Q.Trim();
                q = q.Where(a =>
                    a.pet.name.Contains(kw) ||
                    (a.doctor != null && a.doctor.full_name.Contains(kw)) ||
                    (a.notes != null && a.notes.Contains(kw)) ||
                    (a.status != null && a.status.Contains(kw))
                );
            }

            var projected = q
                .OrderByDescending(a => a.appointment_date)
                .Select(a => new CusAppointmentHistoryItemVM
                {
                    AppointmentId = a.appointment_id,
                    AppointmentDate = a.appointment_date,
                    PetName = a.pet.name,
                    DoctorName =
                        (a.doctor_id != null && a.doctor != null && a.doctor.role_id == 3)
                            ? a.doctor.full_name
                            : "Chưa phân công",
                    Status = a.status ?? "",
                    Notes = a.notes,
                    ServiceNames = string.Join(", ", a.AppointmentDetails.Select(d => d.service.service_name)),
                    TotalAmount = a.Invoice != null ? a.Invoice.total_amount : null
                });

            return await projected.ToPagedResultAsync(query.Page, query.PageSize);
        }

        // lịch đã đặt
        public async Task<PagedResult<CusBookedAppointmentItemVM>>
            GetCusBookedAppointmentsAsync(int customerId, CusBookedAppointmentQuery query)
        {
            var q = _context.Appointments
                .AsNoTracking()
                .Where(a => a.customer_id == customerId);

            // Lịch đã đặt: loại "đã thanh toán" và "đã hủy" "không đến "
            q = q.Where(a => a.status == null
                || (a.status.Trim().ToLower() != "đã thanh toán"
                    && a.status.Trim().ToLower() != "đã hủy"
                    && a.status.Trim().ToLower() != "không đến"));

            // Filter ngày
            if (query.FromDate.HasValue)
                q = q.Where(a => a.appointment_date >= query.FromDate.Value);

            if (query.ToDate.HasValue)
                q = q.Where(a => a.appointment_date <= query.ToDate.Value);

            // Filter theo dịch vụ
            if (query.ServiceId.HasValue)
            {
                q = q.Where(a => a.AppointmentDetails.Any(d => d.service_id == query.ServiceId.Value));
            }

            // Search keyword
            if (!string.IsNullOrWhiteSpace(query.Q))
            {
                var kw = query.Q.Trim();
                q = q.Where(a =>
                    a.pet.name.Contains(kw) ||
                    (a.doctor != null && a.doctor.full_name.Contains(kw)) ||
                    (a.notes != null && a.notes.Contains(kw)) ||
                    (a.status != null && a.status.Contains(kw))
                );
            }

            var projected = q
                .OrderBy(a => a.appointment_date) // sắp tới lên đầu
                .Select(a => new CusBookedAppointmentItemVM
                {
                    AppointmentId = a.appointment_id,
                    AppointmentDate = a.appointment_date,
                    PetName = a.pet.name,
                    DoctorName =
                        (a.doctor_id != null && a.doctor != null && a.doctor.role_id == 3)
                            ? a.doctor.full_name
                            : "Chưa phân công",
                    Status = a.status ?? "",
                    Notes = a.notes,
                    ServiceNames = string.Join(", ", a.AppointmentDetails.Select(d => d.service.service_name)),
                    TotalAmount = a.Invoice != null ? a.Invoice.total_amount : null
                });

            return await projected.ToPagedResultAsync(query.Page, query.PageSize);
        }

        // =========================
        // ✅ NEW: Pets dropdown
        // =========================
        public async Task<List<(int PetId, string PetName)>> GetCustomerPetsAsync(int customerId)
        {
            return await _context.Pets
                .AsNoTracking()
                .Where(p => p.owner_id == customerId)
                .OrderBy(p => p.name)
                .Select(p => new ValueTuple<int, string>(p.pet_id, p.name))
                .ToListAsync();
        }

        // =========================
        // ✅ NEW: Doctors dropdown
        // =========================
        public async Task<List<(int DoctorId, string DoctorName)>> GetDoctorsAsync()
        {
            return await _context.Users
                .AsNoTracking()
                .Where(u => u.role_id == 3)
                .OrderBy(u => u.full_name)
                .Select(u => new ValueTuple<int, string>(u.user_id, u.full_name))
                .ToListAsync();
        }

        // ==========================================================
        // ✅ NEW: Get doctor shifts (2 overloads)
        // ==========================================================

        public Task<List<DoctorShiftVM>> GetDoctorShiftsAsync(int doctorId, DateTime day)
            => GetDoctorShiftsAsync(doctorId, day.Date, day.Date);

        public async Task<List<DoctorShiftVM>> GetDoctorShiftsAsync(int doctorId, DateTime from, DateTime to)
        {
            var targetDate = DateOnly.FromDateTime(from.Date);

            var schedules = await _context.Schedules
                .AsNoTracking()
                .Where(s => s.doctor_id == doctorId
                            && s.work_date == targetDate
                            && (s.status == null
                                || s.status.Trim().ToLower() == "active"
                                || s.status.Trim().ToLower() == "đang làm"
                                || s.status.Trim().ToLower() == "dang lam"))
                .ToListAsync();

            var result = new List<DoctorShiftVM>();

            foreach (var s in schedules)
            {
                var shiftKey = (s.shift ?? "").Trim().ToLower();

                TimeOnly start;
                TimeOnly end;

                switch (shiftKey)
                {
                    case "sáng":
                    case "sang":
                        start = new TimeOnly(8, 0);
                        end = new TimeOnly(12, 0);
                        break;

                    case "chiều":
                    case "chieu":
                        start = new TimeOnly(13, 0);
                        end = new TimeOnly(17, 0);
                        break;

                    case "tối":
                    case "toi":
                        start = new TimeOnly(18, 0);
                        end = new TimeOnly(21, 0);
                        break;

                    case "cả ngày":
                    case "ca ngay":
                    case "cangay":
                        start = new TimeOnly(8, 0);
                        end = new TimeOnly(21, 0);
                        break;

                    default:
                        continue;
                }

                var startDt = targetDate.ToDateTime(start);
                var endDt = targetDate.ToDateTime(end);

                result.Add(new DoctorShiftVM
                {
                    Start = startDt,
                    End = endDt
                });
            }

            return result.OrderBy(x => x.Start).ToList();
        }

        // =========================
        // ✅ NEW: Validate appointment time in doctor's shifts
        // =========================
        public async Task<bool> IsDoctorWorkingAtAsync(int doctorId, DateTime appointmentDateTime)
        {
            var shifts = await GetDoctorShiftsAsync(doctorId, appointmentDateTime.Date);
            return shifts.Any(x => appointmentDateTime >= x.Start && appointmentDateTime < x.End);
        }

        // =========================
        // ✅ NEW: Create Appointment (doctor optional + validate shift)
        // =========================
        public async Task<int> CreateAppointmentAsync(int customerId, CusCreateAppointmentCommand cmd)
        {
            var petOk = await _context.Pets
                .AsNoTracking()
                .AnyAsync(p => p.pet_id == cmd.PetId && p.owner_id == customerId);

            if (!petOk)
                throw new Exception("Thú cưng không hợp lệ (không thuộc tài khoản).");

            var serviceCount = await _context.Services
                .AsNoTracking()
                .CountAsync(s => cmd.ServiceIds.Contains(s.service_id));

            if (serviceCount != cmd.ServiceIds.Count)
                throw new Exception("Danh sách dịch vụ không hợp lệ.");

            if (cmd.AppointmentDate <= DateTime.Now)
                throw new Exception("Ngày giờ khám phải lớn hơn hiện tại.");

            var duplicated = await _context.Appointments
                .AsNoTracking()
                .AnyAsync(a => a.customer_id == customerId
                            && a.pet_id == cmd.PetId
                            && a.appointment_date == cmd.AppointmentDate);

            if (duplicated)
                throw new Exception("Bạn đã có lịch cho thú cưng này tại thời điểm đó.");

            if (cmd.DoctorId.HasValue)
            {
                var doctorOk = await _context.Users.AsNoTracking()
                    .AnyAsync(u => u.user_id == cmd.DoctorId.Value && u.role_id == 3);

                if (!doctorOk)
                    throw new Exception("Bác sĩ không hợp lệ.");

                var inShift = await IsDoctorWorkingAtAsync(cmd.DoctorId.Value, cmd.AppointmentDate);
                if (!inShift)
                    throw new Exception("Giờ đặt không nằm trong ca làm của bác sĩ đã chọn.");
            }

            var appt = new Appointment
            {
                customer_id = customerId,
                pet_id = cmd.PetId,
                appointment_date = cmd.AppointmentDate,
                notes = cmd.Notes,
                status = "Chờ xác nhận",
                doctor_id = cmd.DoctorId
            };

            _context.Appointments.Add(appt);
            await _context.SaveChangesAsync();

            var details = cmd.ServiceIds.Select(sid => new AppointmentDetail
            {
                appointment_id = appt.appointment_id,
                service_id = sid
            }).ToList();

            _context.AppointmentDetails.AddRange(details);
            await _context.SaveChangesAsync();

            return appt.appointment_id;
        }

        // ==========================================================
        // ✅ NEW: Appointment Details / Edit / Cancel (Customer)
        // ==========================================================

        public async Task<CusAppointmentDetailVM?> GetCusAppointmentDetailAsync(int customerId, int appointmentId)
        {
            var q = _context.Appointments
                .AsNoTracking()
                .Where(a => a.customer_id == customerId && a.appointment_id == appointmentId)
                .Select(a => new CusAppointmentDetailVM
                {
                    AppointmentId = a.appointment_id,
                    AppointmentDate = a.appointment_date,
                    CreatedAt = EF.Property<DateTime>(a, CREATED_AT_FIELD),
                    Status = a.status ?? "",
                    Notes = a.notes,
                    PetName = a.pet.name,
                    DoctorName =
                        (a.doctor_id != null && a.doctor != null && a.doctor.role_id == 3)
                            ? a.doctor.full_name
                            : "Chưa phân công",
                    Services = a.AppointmentDetails
                        .Select(d => d.service.service_name)
                        .ToList(),
                    TotalAmount = a.Invoice != null ? a.Invoice.total_amount : null
                });

            return await q.FirstOrDefaultAsync();
        }

        public async Task<CusEditAppointmentVM?> GetCusEditAppointmentAsync(int customerId, int appointmentId)
        {
            var q = _context.Appointments
                .AsNoTracking()
                .Where(a => a.customer_id == customerId && a.appointment_id == appointmentId)
                .Select(a => new CusEditAppointmentVM
                {
                    AppointmentId = a.appointment_id,
                    AppointmentDate = a.appointment_date,
                    Notes = a.notes,
                    Status = a.status ?? "",
                    CreatedAt = EF.Property<DateTime>(a, CREATED_AT_FIELD),
                    ServiceIds = a.AppointmentDetails.Select(d => d.service_id).ToList()
                });

            return await q.FirstOrDefaultAsync();
        }

        public async Task<bool> UpdateCusAppointmentAsync(int customerId, CusEditAppointmentVM vm)
        {
            var appt = await _context.Appointments
                .Include(a => a.AppointmentDetails)
                .FirstOrDefaultAsync(a => a.customer_id == customerId && a.appointment_id == vm.AppointmentId);

            if (appt == null) return false;

            // ✅ KHÔNG CHO SỬA STATUS

            appt.appointment_date = vm.AppointmentDate;
            appt.notes = vm.Notes;

            // ✅ Update services: remove old + add new (đảm bảo xóa thật trong DB)
            if (vm.ServiceIds != null && vm.ServiceIds.Count > 0)
            {
                var validCount = await _context.Services.AsNoTracking()
                    .CountAsync(s => vm.ServiceIds.Contains(s.service_id));

                if (validCount != vm.ServiceIds.Count)
                    throw new Exception("Danh sách dịch vụ không hợp lệ.");

                if (appt.AppointmentDetails != null && appt.AppointmentDetails.Count > 0)
                    _context.AppointmentDetails.RemoveRange(appt.AppointmentDetails);

                var newDetails = vm.ServiceIds.Distinct().Select(sid => new AppointmentDetail
                {
                    appointment_id = appt.appointment_id,
                    service_id = sid
                }).ToList();

                await _context.AppointmentDetails.AddRangeAsync(newDetails);
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> CancelCusAppointmentAsync(int customerId, int appointmentId, string reason)
        {
            var appt = await _context.Appointments
                .FirstOrDefaultAsync(a => a.customer_id == customerId && a.appointment_id == appointmentId);

            if (appt == null) return false;

            appt.status = "đã hủy";

            var old = appt.notes ?? "";
            var line = $"[Lý do hủy]: {reason}";
            appt.notes = string.IsNullOrWhiteSpace(old) ? line : (old + "\n" + line);

            await _context.SaveChangesAsync();
            return true;
        }
    }
}