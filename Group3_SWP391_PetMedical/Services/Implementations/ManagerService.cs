using Microsoft.EntityFrameworkCore;
using Group3_SWP391_PetMedical.Models;
using Group3_SWP391_PetMedical.Models.Common;
using Group3_SWP391_PetMedical.Repository.Interfaces;
using Group3_SWP391_PetMedical.Services.Interfaces;
using Group3_SWP391_PetMedical.ViewModels.Manager;

namespace Group3_SWP391_PetMedical.Services.Implementations
{
    public class ManagerService : IManagerService
    {
        private readonly IServiceRepository _serviceRepo;
        private readonly PetClinicContext _context;

        public ManagerService(IServiceRepository serviceRepo, PetClinicContext context)
        {
            _serviceRepo = serviceRepo;
            _context = context;
        }

        // ========== SERVICES ==========
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

        // ========== SCHEDULE CHANGE REQUESTS (bảng riêng ScheduleChangeRequests) ==========
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
    }
}
