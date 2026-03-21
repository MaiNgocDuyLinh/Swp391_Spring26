using Group3_SWP391_PetMedical.Data;
using Group3_SWP391_PetMedical.Models;
using Group3_SWP391_PetMedical.Models.Common;
using Group3_SWP391_PetMedical.Repository.Interfaces;
using Group3_SWP391_PetMedical.ViewModels.Feedback;
using Microsoft.EntityFrameworkCore;

namespace Group3_SWP391_PetMedical.Repository.Implementations
{
    public class FeedbackRepository : IFeedbackRepository
    {
        private readonly PetClinicContext _context;

        public FeedbackRepository(PetClinicContext context)
        {
            _context = context;
        }

        #region --- Customer Side Methods (from HEAD) ---

        public async Task<CusCreateFeedbackVM?> GetCusCreateFeedbackAsync(int customerId, int appointmentId)
        {
            var q = _context.Appointments
                .AsNoTracking()
                .Where(a => a.customer_id == customerId && a.appointment_id == appointmentId)
                .Select(a => new CusCreateFeedbackVM
                {
                    AppointmentId = a.appointment_id,
                    AppointmentDate = a.appointment_date,
                    PetName = a.pet.name,
                    DoctorName = (a.doctor_id != null && a.doctor != null && a.doctor.role_id == 3)
                        ? a.doctor.full_name
                        : "Chưa phân công",
                    ServiceNames = string.Join(", ", a.AppointmentDetails.Select(d => d.service.service_name))
                });

            return await q.FirstOrDefaultAsync();
        }

        public async Task<bool> HasFeedbackAsync(int customerId, int appointmentId)
        {
            return await _context.Feedback // Đã đổi từ Feedbacks thành Feedback cho đồng bộ
                .AsNoTracking()
                .AnyAsync(f => f.customer_id == customerId && f.appointment_id == appointmentId);
        }

        public async Task<int> CreateFeedbackAsync(int customerId, CusCreateFeedbackVM vm)
        {
            var appointment = await _context.Appointments
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.customer_id == customerId && a.appointment_id == vm.AppointmentId);

            if (appointment == null)
                throw new Exception("Không tìm thấy lịch hẹn.");

            if ((appointment.status ?? "").Trim().ToLower() != "đã thanh toán")
                throw new Exception("Chỉ được feedback lịch hẹn có trạng thái 'Đã thanh toán'.");

            var existed = await _context.Feedback
                .AsNoTracking()
                .AnyAsync(f => f.customer_id == customerId && f.appointment_id == vm.AppointmentId);

            if (existed)
                throw new Exception("Bạn đã feedback lịch hẹn này rồi.");

            var feedback = new Feedback
            {
                appointment_id = vm.AppointmentId,
                customer_id = customerId,
                rating = vm.Rating,
                comment = vm.Comment,
                created_at = DateTime.Now
            };

            _context.Feedback.Add(feedback);
            await _context.SaveChangesAsync();

            return feedback.feedback_id;
        }

        public async Task<PagedResult<CusFeedbackHistoryItemVM>> GetCusFeedbackHistoryAsync(int customerId, CusFeedbackHistoryQuery query)
        {
            var q = _context.Appointments
                .AsNoTracking()
                .Where(a => a.customer_id == customerId)
                .Where(a => a.status != null && a.status.Trim().ToLower() == "đã thanh toán");

            // Filter logic
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
                    a.AppointmentDetails.Any(d => d.service.service_name.Contains(kw)));
            }

            var projected = q
                .OrderByDescending(a => a.appointment_date)
                .Select(a => new CusFeedbackHistoryItemVM
                {
                    AppointmentId = a.appointment_id,
                    AppointmentDate = a.appointment_date,
                    PetName = a.pet.name,
                    DoctorName = (a.doctor_id != null && a.doctor != null && a.doctor.role_id == 3)
                            ? a.doctor.full_name
                            : "Chưa phân công",
                    ServiceNames = string.Join(", ", a.AppointmentDetails.Select(d => d.service.service_name)),
                    Status = a.status ?? "",
                    Notes = a.notes,
                    TotalAmount = a.Invoice != null ? a.Invoice.total_amount : null,

                    HasFeedback = _context.Feedback.Any(f =>
                        f.appointment_id == a.appointment_id &&
                        f.customer_id == customerId),

                    Rating = _context.Feedback
                        .Where(f => f.appointment_id == a.appointment_id && f.customer_id == customerId)
                        .Select(f => f.rating)
                        .FirstOrDefault(),

                    Comment = _context.Feedback
                        .Where(f => f.appointment_id == a.appointment_id && f.customer_id == customerId)
                        .Select(f => f.comment)
                        .FirstOrDefault(),

                    FeedbackCreatedAt = _context.Feedback
                        .Where(f => f.appointment_id == a.appointment_id && f.customer_id == customerId)
                        .Select(f => f.created_at)
                        .FirstOrDefault()
                });

            return await projected.ToPagedResultAsync(query.Page, query.PageSize);
        }

        #endregion

        #region --- Admin/General Methods (from main) ---

        public async Task<PagedResult<Feedback>> GetPagedAsync(string? search, int? starFilter, int page, int pageSize)
        {
            var query = _context.Feedback
                .Include(f => f.customer)
                .Include(f => f.appointment)
                    .ThenInclude(a => a.doctor)
                .Include(f => f.appointment)
                    .ThenInclude(a => a.pet)
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();
                query = query.Where(f =>
                    EF.Functions.Collate(f.comment ?? "", "Vietnamese_CI_AI").Contains(search) ||
                    EF.Functions.Collate(f.customer.full_name ?? "", "Vietnamese_CI_AI").Contains(search) ||
                    EF.Functions.Collate(f.appointment.pet.name ?? "", "Vietnamese_CI_AI").Contains(search)
                );
            }

            if (starFilter.HasValue && starFilter > 0)
            {
                query = query.Where(f => f.rating == starFilter.Value);
            }

            var total = await query.CountAsync();
            var items = await query
                .OrderByDescending(f => f.created_at)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<Feedback>
            {
                Items = items,
                Page = page,
                PageSize = pageSize,
                TotalItems = total
            };
        }

        public async Task<List<Feedback>> GetTopFeedbacksAsync(int count)
        {
            return await _context.Feedback
                .Include(f => f.customer)
                .Where(f => f.rating >= 4 && !string.IsNullOrWhiteSpace(f.comment))
                .OrderByDescending(f => f.created_at)
                .Take(count)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<Feedback?> GetByIdAsync(int id)
        {
            return await _context.Feedback
                .Include(f => f.customer)
                .Include(f => f.appointment)
                    .ThenInclude(a => a.doctor)
                .Include(f => f.appointment)
                    .ThenInclude(a => a.pet)
                .Include(f => f.appointment)
                    .ThenInclude(a => a.AppointmentDetails)
                        .ThenInclude(d => d.service)
                .AsNoTracking()
                .FirstOrDefaultAsync(f => f.feedback_id == id);
        }

        #endregion
    }
}