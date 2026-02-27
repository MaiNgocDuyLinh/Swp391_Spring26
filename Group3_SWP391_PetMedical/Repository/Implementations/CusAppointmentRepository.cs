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

            // ✅ Lịch sử: chỉ lấy lịch có trạng thái "đã thanh toán" hoặc "đã hủy"
            q = q.Where(a => a.status != null &&
                (a.status.Trim().ToLower() == "đã thanh toán" ||
                 a.status.Trim().ToLower() == "đã hủy"));

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
                    //DoctorName = a.doctor != null ? a.doctor.full_name : "Chưa phân công",
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

    }
}