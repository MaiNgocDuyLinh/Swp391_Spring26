using Microsoft.EntityFrameworkCore;
using Group3_SWP391_PetMedical.Models;
using Group3_SWP391_PetMedical.Models.Common;
using Group3_SWP391_PetMedical.Repository.Interfaces;
using Group3_SWP391_PetMedical.Services.Interfaces;
using Group3_SWP391_PetMedical.ViewModels.Manager;

namespace Group3_SWP391_PetMedical.Services.Implementations
{
    public class ManagerServiceHung : IManagerModuleService
    {
        private readonly IServiceRepository _serviceRepo;
        private readonly PetClinicContext _context;

        public ManagerServiceHung(IServiceRepository serviceRepo, PetClinicContext context)
        {
            _serviceRepo = serviceRepo;
            _context = context;
        }

        public Task<PagedResult<Service>> GetServicesPagedAsync(string? search, int page, int pageSize)
        {
            return _serviceRepo.GetPagedAsync(search, page, pageSize);
        }

        public Task<Service?> GetServiceByIdAsync(int id)
        {
            return _serviceRepo.GetByIdAsync(id);
        }

        public Task<bool> UpdateServiceAsync(int id, decimal basePrice, string? description)
        {
            return _serviceRepo.UpdateAsync(id, basePrice, description);
        }

        public async Task<List<ScheduleChangeRequestListVM>> GetScheduleChangeRequestsAsync(string? status = "Pending")
        {
            var query = _context.ScheduleChangeRequests
                .AsNoTracking()
                .Include(r => r.doctor)
                .Include(r => r.schedule)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(status))
            {
                var statusNorm = status.Trim();
                query = query.Where(r => r.status == statusNorm);
            }

            var list = await query
                .OrderByDescending(r => r.created_at)
                .Select(r => new ScheduleChangeRequestListVM
                {
                    RequestId = r.request_id,
                    DoctorId = r.doctor_id,
                    DoctorName = r.doctor.full_name,
                    ScheduleId = r.schedule_id,
                    CurrentWorkDate = r.schedule.work_date,
                    CurrentShift = r.schedule.shift,
                    RequestedWorkDate = r.requested_work_date,
                    RequestedShift = r.requested_shift,
                    Reason = r.reason,
                    Status = r.status ?? "",
                    CreatedAt = r.created_at
                })
                .ToListAsync();
            return list;
        }

        public async Task<ScheduleChangeRequestDetailVM?> GetScheduleChangeRequestByIdAsync(int requestId)
        {
            var r = await _context.ScheduleChangeRequests
                .AsNoTracking()
                .Include(x => x.doctor)
                .Include(x => x.schedule)
                .FirstOrDefaultAsync(x => x.request_id == requestId);
            if (r == null) return null;

            return new ScheduleChangeRequestDetailVM
            {
                RequestId = r.request_id,
                DoctorId = r.doctor_id,
                DoctorName = r.doctor.full_name,
                DoctorPhone = r.doctor.phone,
                ScheduleId = r.schedule_id,
                CurrentWorkDate = r.schedule.work_date,
                CurrentShift = r.schedule.shift,
                RequestedWorkDate = r.requested_work_date,
                RequestedShift = r.requested_shift,
                Reason = r.reason,
                Status = r.status ?? "",
                CreatedAt = r.created_at,
                DecidedAt = r.decided_at,
                ManagerNote = r.manager_note
            };
        }

        public async Task<bool> ApproveScheduleChangeRequestAsync(int requestId, int managerUserId, string? managerNote)
        {
            var req = await _context.ScheduleChangeRequests
                .Include(r => r.schedule)
                .FirstOrDefaultAsync(r => r.request_id == requestId);
            if (req == null || (req.status ?? "").Trim().Equals("Approved", StringComparison.OrdinalIgnoreCase)
                || (req.status ?? "").Trim().Equals("Rejected", StringComparison.OrdinalIgnoreCase))
                return false;

            req.status = "Approved";
            req.decided_at = DateTime.Now;
            req.decided_by = managerUserId;
            req.manager_note = managerNote;

            req.schedule.work_date = req.requested_work_date;
            req.schedule.shift = req.requested_shift;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RejectScheduleChangeRequestAsync(int requestId, int managerUserId, string? managerNote)
        {
            var req = await _context.ScheduleChangeRequests
                .FirstOrDefaultAsync(r => r.request_id == requestId);
            if (req == null || (req.status ?? "").Trim().Equals("Approved", StringComparison.OrdinalIgnoreCase)
                || (req.status ?? "").Trim().Equals("Rejected", StringComparison.OrdinalIgnoreCase))
                return false;

            req.status = "Rejected";
            req.decided_at = DateTime.Now;
            req.decided_by = managerUserId;
            req.manager_note = managerNote;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<OverviewStatsVM> GetOverviewStatsAsync(DateTime? fromDate, DateTime? toDate, string? groupBy)
        {
            if (!fromDate.HasValue || !toDate.HasValue)
            {
                toDate = DateTime.Today;
                fromDate = DateTime.Today.AddDays(-30);
            }
            if (string.IsNullOrWhiteSpace(groupBy)) groupBy = "day";

            var model = new OverviewStatsVM
            {
                FromDate = fromDate,
                ToDate = toDate,
                GroupBy = groupBy
            };

            // Revenue: tổng hóa đơn đã thanh toán
            var invoiceQuery = _context.Invoices.AsNoTracking()
                .Where(i => i.created_at >= fromDate && i.created_at <= toDate);
            var paidInvoices = await invoiceQuery
                .Where(i => i.payment_status != null && i.payment_status.Trim().ToLower() == "paid")
                .ToListAsync();
            model.Revenue = paidInvoices.Sum(i => i.total_amount);

            // CustomerLoginCount: số lần đăng nhập của Customer (qua AuditLog Action=Login)
            var customerUserIds = await _context.Users.AsNoTracking()
                .Where(u => u.role != null && u.role.role_name == "Customer")
                .Select(u => u.user_id)
                .ToListAsync();
            model.CustomerLoginCount = await _context.AuditLogs.AsNoTracking()
                .Where(a => a.Action == "Login" && a.UserId != null && customerUserIds.Contains(a.UserId.Value))
                .Where(a => a.CreatedAt >= fromDate && a.CreatedAt <= toDate)
                .CountAsync();

            // Appointments
            var apptQuery = _context.Appointments.AsNoTracking()
                .Where(a => a.appointment_date >= fromDate && a.appointment_date <= toDate.Value.AddDays(1));
            var appointments = await apptQuery.ToListAsync();
            model.TotalAppointments = appointments.Count;
            model.AppointmentsByStatus = appointments
                .Where(a => !string.IsNullOrEmpty(a.status))
                .GroupBy(a => a.status!)
                .ToDictionary(g => g.Key, g => g.Count());

            // Revenue & Appointments by date for charts
            if (groupBy == "month")
            {
                var revByMonth = paidInvoices.Where(i => i.created_at.HasValue)
                    .GroupBy(i => new DateTime(i.created_at!.Value.Year, i.created_at.Value.Month, 1))
                    .ToDictionary(g => g.Key, g => g.Sum(x => x.total_amount));
                var apptByMonth = appointments
                    .GroupBy(a => new DateTime(a.appointment_date.Year, a.appointment_date.Month, 1))
                    .ToDictionary(g => g.Key, g => g.Count());
                var allMonths = revByMonth.Keys.Union(apptByMonth.Keys).OrderBy(d => d).ToList();
                foreach (var d in allMonths)
                {
                    revByMonth.TryGetValue(d, out var rev);
                    apptByMonth.TryGetValue(d, out var cnt);
                    model.RevenueByDate.Add(new RevenueByDateItem { Label = d.ToString("MM/yyyy"), Value = rev });
                    model.AppointmentsByDate.Add(new AppointmentsByDateItem { Label = d.ToString("MM/yyyy"), Count = cnt });
                }
            }
            else
            {
                var invoicesByDate = paidInvoices.Where(i => i.created_at.HasValue)
                    .GroupBy(i => i.created_at!.Value.Date).ToDictionary(g => g.Key, g => g.Sum(x => x.total_amount));
                var apptsByDate = appointments.GroupBy(a => a.appointment_date.Date).ToDictionary(g => g.Key, g => g.Count());
                var allDates = invoicesByDate.Keys.Union(apptsByDate.Keys).OrderBy(d => d).ToList();
                foreach (var d in allDates)
                {
                    invoicesByDate.TryGetValue(d, out var rev);
                    apptsByDate.TryGetValue(d, out var cnt);
                    model.RevenueByDate.Add(new RevenueByDateItem { Label = d.ToString("dd/MM"), Value = rev });
                    model.AppointmentsByDate.Add(new AppointmentsByDateItem { Label = d.ToString("dd/MM"), Count = cnt });
                }
            }

            return model;
        }
    }
}

