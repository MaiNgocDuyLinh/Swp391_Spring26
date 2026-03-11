using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

            q = q.Where(a => a.status != null &&
                (a.status.Trim().ToLower() == "đã thanh toán" ||
                 a.status.Trim().ToLower() == "đã hủy" ||
                 a.status.Trim().ToLower() == "không đến"));

            if (query.FromDate.HasValue)
                q = q.Where(a => a.appointment_date >= query.FromDate.Value);

            if (query.ToDate.HasValue)
                q = q.Where(a => a.appointment_date <= query.ToDate.Value);

            if (query.ServiceId.HasValue)
                q = q.Where(a => a.AppointmentDetails.Any(d => d.service_id == query.ServiceId.Value));

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
                     .OrderByDescending(a => a.status != null && a.status.Trim().ToLower() == "đã hủy") 
                     .ThenByDescending(a => EF.Property<DateTime?>(a, CREATED_AT_FIELD) != null)        
                     .ThenByDescending(a => EF.Property<DateTime?>(a, CREATED_AT_FIELD))                
                     .ThenByDescending(a => a.appointment_id)                                          
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

        public async Task<PagedResult<CusBookedAppointmentItemVM>>
            GetCusBookedAppointmentsAsync(int customerId, CusBookedAppointmentQuery query)
        {
            var q = _context.Appointments
                .AsNoTracking()
                .Where(a => a.customer_id == customerId);

            q = q.Where(a => a.status == null
             || (a.status.Trim().ToLower() != "đã thanh toán"
                 && a.status.Trim().ToLower() != "đã hủy"
                 && a.status.Trim().ToLower() != "không đến"));

            if (query.FromDate.HasValue)
                q = q.Where(a => a.appointment_date >= query.FromDate.Value);

            if (query.ToDate.HasValue)
                q = q.Where(a => a.appointment_date <= query.ToDate.Value);

            if (query.ServiceId.HasValue)
                q = q.Where(a => a.AppointmentDetails.Any(d => d.service_id == query.ServiceId.Value));

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
                .OrderByDescending(a => EF.Property<DateTime?>(a, CREATED_AT_FIELD) != null)
                .ThenByDescending(a => EF.Property<DateTime?>(a, CREATED_AT_FIELD))
                .ThenByDescending(a => a.appointment_id)
                .Select(a => new CusBookedAppointmentItemVM
                {
                    AppointmentId = a.appointment_id,
                    AppointmentDate = a.appointment_date,
                    //CreatedAt = EF.Property<DateTime>(a, "created_at"),
                    CreatedAt = EF.Property<DateTime?>(a, "created_at") ?? a.appointment_date,
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

        public async Task<List<(int PetId, string PetName)>> GetCustomerPetsAsync(int customerId)
        {
            return await _context.Pets
                .AsNoTracking()
                .Where(p => p.owner_id == customerId)
                .OrderBy(p => p.name)
                .Select(p => new ValueTuple<int, string>(p.pet_id, p.name))
                .ToListAsync();
        }

        public async Task<List<(int DoctorId, string DoctorName)>> GetDoctorsAsync()
        {
            return await _context.Users
                .AsNoTracking()
                .Where(u => u.role_id == 3)
                .OrderBy(u => u.full_name)
                .Select(u => new ValueTuple<int, string>(u.user_id, u.full_name))
                .ToListAsync();
        }

        public Task<List<DoctorShiftVM>> GetDoctorShiftsAsync(int doctorId, DateTime day)
            => GetDoctorShiftsAsync(doctorId, day.Date, day.Date);


        public async Task<List<DoctorAppointmentEventVM>>
     GetDoctorAppointmentsAsync(int doctorId, DateTime from, DateTime to)
        {
            var q = _context.Appointments
                .AsNoTracking()
                .Where(a => a.doctor_id == doctorId
                            && a.appointment_date >= from
                            && a.appointment_date <= to
                            && (a.status == null || a.status.Trim().ToLower() != "đã hủy"));

            var raw = await q
                .OrderBy(a => a.appointment_date)
                .Select(a => new
                {
                    PetName = a.pet != null ? a.pet.name : "Lịch hẹn",
                    AppointmentDate = a.appointment_date,
                    Status = a.status
                })
                .ToListAsync();

            var list = raw
                .Select(a =>
                {
                    var slot = GetAppointmentSlot(a.AppointmentDate);

                    return new DoctorAppointmentEventVM
                    {
                        Title = $"{slot.Shift} - {a.PetName}",
                        Start = slot.Start,
                        End = slot.End,
                        Status = a.Status
                    };
                })
                .OrderBy(x => x.Start)
                .ToList();

            return list;
        }
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
                foreach (var slot in ExpandScheduleSlots(s.shift))
                {
                    result.Add(new DoctorShiftVM
                    {
                        Start = targetDate.ToDateTime(slot.Start),
                        End = targetDate.ToDateTime(slot.End),
                        Shift = slot.Shift
                    });
                }
            }

            return result
                .GroupBy(x => new { x.Start, x.End, x.Shift })
                .Select(g => g.First())
                .OrderBy(x => x.Start)
                .ToList();
        }

        public async Task<bool> IsDoctorWorkingAtAsync(int doctorId, DateTime appointmentDateTime)
        {
            var shifts = await GetDoctorShiftsAsync(doctorId, appointmentDateTime.Date);
            return shifts.Any(x => appointmentDateTime >= x.Start && appointmentDateTime < x.End);
        }

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

            var appointmentDateTime = BuildAppointmentDateTime(cmd.AppointmentDate, cmd.Shift);

            if (appointmentDateTime <= DateTime.Now)
                throw new Exception("Ngày khám phải lớn hơn hiện tại.");

            var duplicated = await _context.Appointments
                .AsNoTracking()
                .AnyAsync(a => a.customer_id == customerId
                            && a.pet_id == cmd.PetId
                            && a.appointment_date == appointmentDateTime);

            if (duplicated)
                throw new Exception("Bạn đã có lịch cho thú cưng này ở ca đó.");

            if (cmd.DoctorId.HasValue)
            {
                var doctorOk = await _context.Users
                    .AsNoTracking()
                    .AnyAsync(u => u.user_id == cmd.DoctorId.Value && u.role_id == 3);

                if (!doctorOk)
                    throw new Exception("Bác sĩ không hợp lệ.");

                var inShift = await IsDoctorWorkingAtAsync(cmd.DoctorId.Value, appointmentDateTime);
                if (!inShift)
                    throw new Exception("Ca khám đã chọn không nằm trong ca làm của bác sĩ.");
            }

            var appt = new Appointment
            {
                customer_id = customerId,
                pet_id = cmd.PetId,
                appointment_date = appointmentDateTime,
                notes = cmd.Notes,
                status = "Chờ xác nhận",
                doctor_id = cmd.DoctorId,
                created_at = DateTime.Now
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

        private static string NormalizeShiftKey(string? shift)
        {
            var key = (shift ?? "").Trim().ToLowerInvariant();

            return key switch
            {
                "sáng" or "sang" => "sáng",
                "chiều" or "chieu" => "chiều",
                "cả ngày" or "ca ngay" or "cangay" => "cả ngày",
                _ => ""
            };
        }

        private static DateTime BuildAppointmentDateTime(DateTime appointmentDate, string? shift)
        {
            var date = appointmentDate.Date;

            return NormalizeShiftKey(shift) switch
            {
                "sáng" => date.AddHours(8),
                "chiều" => date.AddHours(13),
                _ => throw new Exception("Ca khám không hợp lệ.")
            };
        }

        private static IEnumerable<(string Shift, TimeOnly Start, TimeOnly End)> ExpandScheduleSlots(string? shift)
        {
            switch (NormalizeShiftKey(shift))
            {
                case "sáng":
                    yield return ("Ca sáng", new TimeOnly(8, 0), new TimeOnly(12, 0));
                    yield break;

                case "chiều":
                    yield return ("Ca chiều", new TimeOnly(13, 0), new TimeOnly(17, 0));
                    yield break;

                case "cả ngày":
                    yield return ("Ca sáng", new TimeOnly(8, 0), new TimeOnly(12, 0));
                    yield return ("Ca chiều", new TimeOnly(13, 0), new TimeOnly(17, 0));
                    yield break;
            }
        }

        private static (string Shift, DateTime Start, DateTime End) GetAppointmentSlot(DateTime appointmentDateTime)
        {
            var date = appointmentDateTime.Date;

            if (appointmentDateTime.Hour < 12)
            {
                return ("Ca sáng", date.AddHours(8), date.AddHours(12));
            }

            return ("Ca chiều", date.AddHours(13), date.AddHours(17));
        }



        private const int MAX_APPOINTMENTS_PER_SHIFT = 5;

        private static (DateTime ShiftStart, DateTime ShiftEnd, string ShiftLabel) GetShiftWindow(DateTime appointmentDateTime)
        {
            var date = appointmentDateTime.Date;
            var time = appointmentDateTime.TimeOfDay;

            // ca sáng: 08:00 - 12:00
            if (time >= new TimeSpan(8, 0, 0) && time < new TimeSpan(12, 0, 0))
            {
                return (date.AddHours(8), date.AddHours(12), "ca sáng");
            }

            // ca chiều: 13:00 - 17:00
            if (time >= new TimeSpan(13, 0, 0) && time < new TimeSpan(17, 0, 0))
            {
                return (date.AddHours(13), date.AddHours(17), "ca chiều");
            }

            throw new Exception("Thời gian khám không thuộc ca sáng hoặc ca chiều.");
        }

        private async Task<int> CountDoctorAppointmentsInShiftAsync(
            int doctorId,
            DateTime appointmentDateTime,
            int? excludeAppointmentId = null)
        {
            var (shiftStart, shiftEnd, _) = GetShiftWindow(appointmentDateTime);

            var q = _context.Appointments
                .AsNoTracking()
                .Where(a => a.doctor_id == doctorId
                            && a.appointment_date >= shiftStart
                            && a.appointment_date < shiftEnd
                            && (a.status == null || a.status.Trim().ToLower() != "đã hủy"));

            if (excludeAppointmentId.HasValue)
            {
                q = q.Where(a => a.appointment_id != excludeAppointmentId.Value);
            }

            return await q.CountAsync();
        }


        public async Task<CusAppointmentDetailVM?> GetCusAppointmentDetailAsync(int customerId, int appointmentId)
        {
            var q = _context.Appointments
                .AsNoTracking()
                .Where(a => a.customer_id == customerId && a.appointment_id == appointmentId)
                .Select(a => new CusAppointmentDetailVM
                {
                    AppointmentId = a.appointment_id,
                    AppointmentDate = a.appointment_date,
                    //CreatedAt = EF.Property<DateTime>(a, CREATED_AT_FIELD),
                    CreatedAt = EF.Property<DateTime?>(a, CREATED_AT_FIELD) ?? a.appointment_date,
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
                    AppointmentDate = a.appointment_date.Date,
                    Shift = a.appointment_date.Hour < 12 ? "sáng" : "chiều",
                    Notes = a.notes,
                    Status = a.status ?? "",
                    CreatedAt = a.created_at ?? a.appointment_date,
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

            if (appt.status != "Chờ xác nhận" && appt.status != "Đặt lịch thành công")
                throw new Exception("Chỉ được chỉnh sửa lịch khi trạng thái là 'Chờ xác nhận' hoặc 'Đặt lịch thành công'.");
            DateTime createdAt;
            try
            {
                DateTime? createdAtNullable;
                try
                {
                    createdAtNullable = _context.Entry(appt).Property<DateTime?>(CREATED_AT_FIELD).CurrentValue;
                }
                catch
                {
                    createdAtNullable = _context.Entry(appt).Property<DateTime>(CREATED_AT_FIELD).CurrentValue;
                }

                createdAt = createdAtNullable ?? appt.created_at ?? appt.appointment_date;
            }
            catch
            {
                throw new Exception($"Không đọc được cột ngày tạo '{CREATED_AT_FIELD}'. Hãy kiểm tra tên cột created_at trong bảng Appointments.");
            }

            if (DateTime.Now - createdAt > TimeSpan.FromHours(24))
                throw new Exception("Chỉ được chỉnh sửa lịch hẹn trong vòng 24 giờ kể từ lúc đặt lịch.");

            if (vm.AppointmentDate == default)
                throw new Exception("Vui lòng chọn ngày khám.");

            if (string.IsNullOrWhiteSpace(vm.Shift))
                throw new Exception("Vui lòng chọn ca khám.");

            var newAppointmentDateTime = BuildAppointmentDateTime(vm.AppointmentDate, vm.Shift);

            if (newAppointmentDateTime <= DateTime.Now)
                throw new Exception("Ngày khám phải lớn hơn hiện tại.");

            var duplicated = await _context.Appointments
                .AsNoTracking()
                .AnyAsync(a => a.customer_id == customerId
                            && a.pet_id == appt.pet_id
                            && a.appointment_id != appt.appointment_id
                            && a.appointment_date == newAppointmentDateTime);

            if (duplicated)
                throw new Exception("Bạn đã có lịch cho thú cưng này ở ca đã chọn.");

            if (appt.doctor_id.HasValue)
            {
                var inShift = await IsDoctorWorkingAtAsync(appt.doctor_id.Value, newAppointmentDateTime);
                if (!inShift)
                    throw new Exception("Ca khám mới không nằm trong ca làm của bác sĩ đã được phân công.");
            }

            appt.appointment_date = newAppointmentDateTime;
            appt.notes = vm.Notes;

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

        // Data cho popup Cancel (ngắn gọn: ngày giờ, thú cưng, dịch vụ)
        public async Task<CusCancelAppointmentVM?> GetCusCancelAppointmentAsync(int customerId, int appointmentId)
        {
            var q = _context.Appointments
                .AsNoTracking()
                .Where(a => a.customer_id == customerId && a.appointment_id == appointmentId)
                .Select(a => new CusCancelAppointmentVM
                {
                    AppointmentId = a.appointment_id,
                    AppointmentDate = a.appointment_date,
                    PetName = a.pet.name,
                    ServiceNames = string.Join(", ", a.AppointmentDetails.Select(d => d.service.service_name)),
                    Description = a.notes
                });

            return await q.FirstOrDefaultAsync();
        }

        public async Task<bool> CancelCusAppointmentAsync(int customerId, int appointmentId, string reason)
        {
            var appt = await _context.Appointments
                .FirstOrDefaultAsync(a => a.customer_id == customerId && a.appointment_id == appointmentId);

            if (appt == null) return false;

            if (appt.status != "Chờ xác nhận" && appt.status != "Đặt lịch thành công")
                throw new Exception("Chỉ được hủy lịch khi trạng thái là 'Chờ xác nhận' hoặc 'Đặt lịch thành công'.");

            if (appt.appointment_date <= DateTime.Now.AddHours(8))
                throw new Exception("Không được hủy lịch khi còn dưới 8 giờ trước giờ khám.");

            appt.status = "Đã hủy";

            var old = appt.notes ?? "";
            var line = $"[Lý do hủy]: {reason}";
            appt.notes = string.IsNullOrWhiteSpace(old) ? line : old + "\n" + line;

            await _context.SaveChangesAsync();
            return true;
        }
    }
}